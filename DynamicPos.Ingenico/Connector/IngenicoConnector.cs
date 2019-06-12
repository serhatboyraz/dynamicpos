using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using DynamicPos.CrdData.Model;
using DynamicPos.CrdService.Interface;
using DynamicPos.Utils.Helpers;
using UmaVendor.CRD.Model;
using SalePaymentDataModel = DynamicPos.CrdData.Model.SalePaymentDataModel;

namespace DynamicPos.Ingenico.Connector
{

    /// <summary>
    /// Olivetti pos cihazı işlemlerinin yapılacağı sınıf.
    /// </summary>
    public class IngenicoConnector : ICrdConnector, IDisposable
    {
        /// <summary>
        /// Pos cihazında yapılan işlemlerin numarası tutulur.
        /// </summary>
        private static ulong ACTIVE_TRX_HANDLE;

        private int ReceiptNumber = 0;

        public bool Connected { get; set; }

        public uint SelectedInterface { get; set; }
        public const string DLL_VERSION_MIN = "1602030800";
        public const int TIMEOUT_DEFAULT = 10000;	// 10 seconds
        public const int TIMEOUT_CARD_TRANSACTIONS = 60000;	// 60 seconds
        public const int TIMEOUT_ECHO = 1000;	// 1 seconds
        public const int TIMEOUT_PRINT_MF = 20000;	// 20 seconds
        public const int TIMEOUT_DATABASE_EXECUTE = 20000;	// 20 seconds
        public const int MAX_UNIQUE_ID = 256;

        private DocumentHeaderResultModel DocumentHeaderResultModel = new DocumentHeaderResultModel();

        public DoAllProcessResultModel DoAllProcess(DoAllProcessModel allProcessModel)
        {

            return null;
        }

        public ConnectionResultModel IsConnected()
        {
            return new ConnectionResultModel()
            {
                StatusCode = Connected ? HttpStatusCode.OK : HttpStatusCode.InternalServerError
            };
        }

        public HandShakeResultModel HandShake(HandShakeModel handShakeModel)
        {
            SetConnectionXmlAttributes(handShakeModel);

            ST_GMP_PAIR pairing = new ST_GMP_PAIR();
            pairing.szExternalDeviceBrand = handShakeModel.DeviceBrand;
            pairing.szExternalDeviceModel = handShakeModel.DeviceModel;
            pairing.szExternalDeviceSerialNumber = handShakeModel.DeviceSerial;
            pairing.szEcrSerialNumber = handShakeModel.DeviceEcrSerial;
            pairing.szProcOrderNumber = "000001";
            pairing.szProcDate = DateTime.Now.ToString("ddMMyy");
            pairing.szProcTime = DateTime.Now.ToString("HHmmss");

            uint[] interfaceList = new uint[1];
            GetInterfaceHandleList(ref interfaceList, (uint)interfaceList.Length);
            ST_GMP_PAIR_RESP pairingResp = new ST_GMP_PAIR_RESP();

            SelectedInterface = interfaceList[0];
            try
            {
                var resp = Json_GMPSmartDLL.FP3_StartPairingInit(SelectedInterface, ref pairing, ref pairingResp,
                    Defines.TIMEOUT_DEFAULT);
                Connected = true;
                return new HandShakeResultModel()
                {
                    HttpStatusCode = resp == 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError
                };
            }
            catch (Exception e)
            {
                return new HandShakeResultModel()
                {
                    HttpStatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public DocumentHeaderResultModel ReceiptBegin(DocumentHeaderModel headerModel)
        {
            StartTicket();
            OptionsFlag();

            ST_INVIOCE_INFO stInvioceInfo = new ST_INVIOCE_INFO();
            stInvioceInfo.source = (byte)2;
            stInvioceInfo.currency = 949;
            stInvioceInfo.no.ConvertAscToBcdArray(ReceiptNumber++.ToString());
            stInvioceInfo.tck_no.ConvertAscToBcdArray(string.IsNullOrEmpty(headerModel.CustomerTcNo) ? "11111111111" : headerModel.CustomerTcNo);
            stInvioceInfo.vk_no.ConvertAscToBcdArray(string.IsNullOrEmpty(headerModel.CustomerVkNo) ? "11111111111" : headerModel.CustomerVkNo);
            stInvioceInfo.date.ConvertStringToHexArray();

            SetInvoice(stInvioceInfo);
            PrintHeader(TTicketType.TInvoice);

            ST_TICKET ticket = GetTicket();

            DocumentHeaderResultModel = new DocumentHeaderResultModel()
            {
                ZNum = ticket.ZNo.ToString(),
                ReceiptNum = ticket.FNo.ToString(),
                StatusCode = HttpStatusCode.OK
            };
            return DocumentHeaderResultModel;
        }

        public SaleItemResultModel ItemSale(SaleItemDataModel saleItemModel)
        {
            for (var i = 0; i < saleItemModel.TransItems.Count; i++)
            {
                ItemSale(saleItemModel.TransItems[i], byte.Parse(saleItemModel.DepartmentIndex));
            }

            return new SaleItemResultModel()
            {
                ZNum = DocumentHeaderResultModel.ZNum,
                ReceiptNo = DocumentHeaderResultModel.ReceiptNum
                //ToDo : transitemsresult yazılacak.
            };
        }

        public GetPaymentResultModel GetPayment(SalePaymentDataModel payment)
        {
            try
            {
                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();
                stPaymentRequest.subtypeOfPayment = 0;
                stPaymentRequest.payAmount = Extensions.ConvertToUint(double.Parse(payment.Amount));
                stPaymentRequest.payAmountCurrencyCode = (UInt16)ECurrency.CURRENCY_TL;

                switch (payment.PaymentTypeCode)
                {
                    case "01":
                        stPaymentRequest.typeOfPayment = (uint)EPaymentTypes.PAYMENT_CASH_TL;
                        break;
                    case "02":
                        stPaymentRequest.typeOfPayment = (uint)EPaymentTypes.PAYMENT_BANK_CARD;
                        break;
                }

                GetPaymentResultModel resultMdl = new GetPaymentResultModel();
                ST_TICKET m_stTicket = new ST_TICKET();

                var retCode = Json_GMPSmartDLL.FP3_Payment(SelectedInterface, ACTIVE_TRX_HANDLE, ref stPaymentRequest, ref m_stTicket, 120000);
                if (retCode == Defines.TRAN_RESULT_OK)
                {

                    if (m_stTicket.TotalReceiptAmount == 0)
                        m_stTicket.TotalReceiptAmount = m_stTicket.invoiceAmount;

                    var display = String.Format("TOPLAM : {0}", FormatAmount(m_stTicket.TotalReceiptAmount, ECurrency.CURRENCY_TL));

                    if (m_stTicket.CashBackAmount != 0)
                        display += String.Format(Environment.NewLine + "P.ÜSTÜ : {0}", FormatAmount(m_stTicket.CashBackAmount, ECurrency.CURRENCY_TL));
                    else if (m_stTicket.TotalReceiptAmount != 0)
                        display += String.Format(Environment.NewLine + "KALAN : {0}", FormatAmount(m_stTicket.TotalReceiptAmount - m_stTicket.TotalReceiptPayment, ECurrency.CURRENCY_TL));
                    else
                        display += String.Format(Environment.NewLine + "ÖDENEN : {0}", FormatAmount(m_stTicket.TotalReceiptPayment, ECurrency.CURRENCY_TL));

                    Logger.Info(display);
                    if ((stPaymentRequest.typeOfPayment == (uint)EPaymentTypes.PAYMENT_BANK_CARD) || (stPaymentRequest.typeOfPayment == (uint)EPaymentTypes.PAYMENT_MOBILE))
                    {
                        resultMdl.AuthCode = m_stTicket.stPayment[0].stBankPayment.authorizeCode;
                        resultMdl.AcquirerId = m_stTicket.stPayment[0].stBankPayment.bankBkmId.ToString();
                        resultMdl.BatchNum = m_stTicket.stPayment[0].stBankPayment.batchNo.ToString();
                        resultMdl.StanNum = m_stTicket.stPayment[0].stBankPayment.stan.ToString();
                        resultMdl.TerminalId = m_stTicket.stPayment[0].stBankPayment.terminalId;
                        resultMdl.MerchantId = m_stTicket.stPayment[0].stBankPayment.merchantId;
                    }

                    if (m_stTicket.TotalReceiptPayment != 0)
                        resultMdl.Amount = Extensions.ConvertToDouble(m_stTicket.TotalReceiptPayment).ToString(CultureInfo.InvariantCulture);
                }
                else
                {

                    throw new CrdException("Ödeme Hatası : " + GetDefineText(retCode));
                }

                return resultMdl;
            }
            catch (Exception e)
            {
                Logger.Error("Get Payment", e);
                throw e;
            }
        }

        public CancelPaymentResultModel CancelPayment(CancelPaymentModel cancelPaymentModel)
        {
            return null;
        }

        public EndReceiptResultModel EndReceipt()
        {
            PrintBeforeMF();
            PrintMF();
            CloseHandle();

            return new EndReceiptResultModel()
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        public CancelDocumentResultModel CancelDocument()
        {
            VoidReceipt();

            return new CancelDocumentResultModel()
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        public SumAmountResultModel GetSumAmount()
        {
            return null;
        }

        public void ChangeMode(string mode)
        {

        }

        public void GetDailyXReport()
        {
        }

        public void GetDailyZReport()
        {
        }

        private void ItemSale(TransItemModel saleItem, byte deptartmentIndex)
        {
            /**
             * SaleItem'daki final amount her zaman türk lirasıdır :*
             */
            ushort currency = 949;
            uint retcode;
            byte unitType = 0;
            UInt32 itemCount = uint.Parse(saleItem.Quantity);
            byte itemCountPrecition = 0;
            ST_TICKET m_stTicket = new ST_TICKET();
            ST_ITEM stItem = new ST_ITEM();

            stItem.type = Defines.ITEM_TYPE_DEPARTMENT;
            stItem.subType = 0;
            stItem.deptIndex = deptartmentIndex;
            stItem.amount = Extensions.ConvertToUint(double.Parse(saleItem.FinalAmount));
            stItem.currency = currency;
            stItem.count = itemCount;
            stItem.unitType = unitType;
            stItem.pluPriceIndex = 0;
            stItem.countPrecition = itemCountPrecition;
            stItem.name = saleItem.MaterialName;
            stItem.barcode = saleItem.EAN11;
            retcode = Json_GMPSmartDLL.FP3_ItemSale(SelectedInterface, ACTIVE_TRX_HANDLE, ref stItem, ref m_stTicket, TIMEOUT_DEFAULT);


            if (retcode != 0)
            {
                throw new CrdException("Ürün satış  hatası: " + GetDefineText(retcode));
            }
        }

        private string FormatAmount(uint amount, ECurrency currency)
        {

            string amountStr = "";

            amountStr = String.Format("{0}.{1:00}", amount / 100, amount % 100);

            switch (currency)
            {
                case ECurrency.CURRENCY_NONE:
                    break;
                case ECurrency.CURRENCY_DOLAR:
                    amountStr += " $";
                    break;
                case ECurrency.CURRENCY_EU:
                    amountStr += " €";
                    break;
                case ECurrency.CURRENCY_TL:
                    amountStr += " TL";
                    break;
                default:
                    amountStr += " ?";
                    break;
            }

            return amountStr;
        }

        private uint GetInterfaceHandleList(ref uint[] InterfaceList, uint InterfaceListLength)
        {
            try
            {
                return GMPSmartDLL.FP3_GetInterfaceHandleList(InterfaceList, InterfaceListLength);

            }
            catch (Exception e)
            {
                Logger.Error("GetInterfaceHandleList", e);
                throw e;

            }
        }

        private void SetConnectionXmlAttributes(HandShakeModel handShakeModel)
        {
            string gmpFilePath = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "GMP.XML");
            if (File.Exists(gmpFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(gmpFilePath);

                XmlNode interfaceNode = xmlDoc.SelectSingleNode("GMP/INTERFACE");
                if (interfaceNode?.Attributes != null)
                    interfaceNode.Attributes["ID"].InnerText = string.Format("\\\\.\\{0}", handShakeModel.ComPort);

                XmlNode portNameNode = xmlDoc.SelectSingleNode("GMP/INTERFACE/PortName");
                if (portNameNode != null)
                    portNameNode.FirstChild.Value = string.Format("\\\\.\\{0}", handShakeModel.ComPort);

                XmlNode connectionTypeNode = xmlDoc.SelectSingleNode("GMP/INTERFACE/IsTcpConnection");
                if (connectionTypeNode != null)
                    connectionTypeNode.FirstChild.Value =
                        (handShakeModel.PosConnectionType != "RS232").ToString().ToUpper();

                XmlNode ipNode = xmlDoc.SelectSingleNode("GMP/INTERFACE/IP");
                if (ipNode != null)
                    ipNode.FirstChild.Value = handShakeModel.PosIp;

                XmlNode portNode = xmlDoc.SelectSingleNode("GMP/INTERFACE/Port");
                if (portNode != null)
                    portNode.FirstChild.Value = handShakeModel.PosIpPort;
            }
        }

        private void StartTicket()
        {
            uint retCode = Defines.TRAN_RESULT_OK;
            var m_uniqueId = new byte[24] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            retCode = GMPSmartDLL.FP3_Start(SelectedInterface, ref ACTIVE_TRX_HANDLE, 0, m_uniqueId, m_uniqueId.Length, null, 0, null, 0, TIMEOUT_DEFAULT);
            HandleErrorCode(retCode);
            if (retCode == Defines.APP_ERR_ALREADY_DONE)
            {
                StartTicket();
            }
        }

        private void PrintHeader(TTicketType headerTicketType)
        {
            var retCode = GMPSmartDLL.FP3_TicketHeader(SelectedInterface, ACTIVE_TRX_HANDLE, headerTicketType, TIMEOUT_DEFAULT);
            HandleErrorCode(retCode);
            if (retCode != 0)
            {
                throw new CrdException("Fiş Başlığı Çıkartma Hatası : " + GetDefineText(retCode));
            }
        }

        private void SetInvoice(ST_INVIOCE_INFO stInvioceInfo)
        {

            ST_TICKET stTicket = new ST_TICKET();

            uint retCode = Json_GMPSmartDLL.FP3_SetInvoice(SelectedInterface, ACTIVE_TRX_HANDLE, ref stInvioceInfo, ref stTicket, 10000);
            byte[] Buffer = new byte[2000];
            GMPSmartDLL.GetErrorMessage(retCode, Buffer);
            var message = Encoding.GetEncoding(65001).GetString(Buffer);
            if (retCode != 0)
            {
                throw new CrdException(string.Format("Fatura fiş başlığı çıkarma hatası : {0}", GetDefineText(retCode)));
            }

        }
        private void OptionsFlag()
        {
            UInt64 activeFlags = 0;

            var retCode = GMPSmartDLL.FP3_OptionFlags(SelectedInterface, ACTIVE_TRX_HANDLE, ref activeFlags, Defines.GMP3_OPTION_ECHO_PRINTER | Defines.GMP3_OPTION_ECHO_ITEM_DETAILS | Defines.GMP3_OPTION_ECHO_PAYMENT_DETAILS, 0, TIMEOUT_DEFAULT);
            HandleErrorCode(retCode);
            if (retCode != 0)
            {
                GMPSmartDLL.FP3_Close(SelectedInterface, 0, Defines.TIMEOUT_DEFAULT);
                string errorString = GetDefineText(retCode);
                throw new CrdException(string.IsNullOrEmpty(errorString) ? "optionsFlag Error: " : errorString);
            }
        }

        private ST_TICKET GetTicket()
        {
            ST_TICKET m_stTicket = new ST_TICKET();
            var retcode = Json_GMPSmartDLL.FP3_GetTicket(SelectedInterface, ACTIVE_TRX_HANDLE, ref m_stTicket, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode != 0)
            {
                throw new CrdException("optionsFlag Error: " + retcode);
            }
            return m_stTicket;
        }

        private void HandleErrorCode(UInt32 errorCode)
        {
            if (errorCode == Defines.APP_ERR_GMP3_INVALID_HANDLE)
            {
                ACTIVE_TRX_HANDLE = 0;
                var m_uniqueId = new byte[24] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                UInt32 retcode = GMPSmartDLL.FP3_Start(SelectedInterface, ref ACTIVE_TRX_HANDLE, 0, m_uniqueId, m_uniqueId.Length, null, 0, null, 0, TIMEOUT_DEFAULT);

                if (retcode == Defines.APP_ERR_ALREADY_DONE)
                    retcode = ReloadTransaction();
            }

            if (errorCode == 2358)
            {
                uint resp = GMPSmartDLL.FP3_Close(SelectedInterface, ACTIVE_TRX_HANDLE, Defines.TIMEOUT_DEFAULT);
                ACTIVE_TRX_HANDLE = 0;
            }
        }

        private UInt32 ReloadTransaction()
        {
            UInt32 RetCode = 0;
            ST_TICKET m_stTicket = new ST_TICKET();
            UInt64 activeFlags = 0;

            RetCode = GMPSmartDLL.FP3_OptionFlags(SelectedInterface, ACTIVE_TRX_HANDLE, ref activeFlags, Defines.GMP3_OPTION_ECHO_PRINTER | Defines.GMP3_OPTION_ECHO_ITEM_DETAILS | Defines.GMP3_OPTION_ECHO_PAYMENT_DETAILS, 0, Defines.TIMEOUT_DEFAULT);
            if (RetCode != Defines.TRAN_RESULT_OK)
                return RetCode;

            RetCode = Json_GMPSmartDLL.FP3_GetTicket(SelectedInterface, ACTIVE_TRX_HANDLE, ref m_stTicket, Defines.TIMEOUT_DEFAULT);
            if (RetCode != Defines.TRAN_RESULT_OK)
                return RetCode;

            return RetCode;
        }

        private string GetDefineText(uint code)
        {
            var define = new Defines();
            foreach (var memberInfo in define.GetType().GetFields())
            {
                int val = 0;
                int.TryParse(memberInfo.GetValue(define).ToString(), out val);
                if (val == code)
                {
                    return memberInfo.Name;
                }
            }

            return string.Empty;
        }

        private void PrintBeforeMF()
        {
            var retCode = GMPSmartDLL.FP3_PrintBeforeMF(SelectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            HandleErrorCode(retCode);
            if (retCode != 0)
            {
                throw new CrdException("printBeforeMF error:" + GetDefineText(retCode));
            }
        }

        private void PrintMF()
        {
            var retCode = GMPSmartDLL.FP3_PrintMF(SelectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            if (retCode != 0)
            {
                throw new CrdException("printMF error:" + GetDefineText(retCode));
            }
        }

        private void CloseHandle()
        {
            var retCode = GMPSmartDLL.FP3_Close(SelectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            ACTIVE_TRX_HANDLE = 0;
            if (retCode != 0)
            {
                throw new CrdException("closeHandle error:" + GetDefineText(retCode));
            }
        }

        private void VoidReceipt()
        {
            ST_TICKET m_stTicket = new ST_TICKET();

            uint retcode = Json_GMPSmartDLL.FP3_VoidAll(SelectedInterface, 0, ref m_stTicket, TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("itemPercentIncrease error: " + retcode);
            }
        }

        public void Dispose()
        {
        }

    }
}
