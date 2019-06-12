using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DynamicPos.CrdService.Interface;
using DynamicPos.Ingenico;
using DynamicPos.Utils.Helpers;
using Newtonsoft.Json;
using UmaVendor.CRD.Model;
using UmaVendor.CRD.Model.DataModel; 
using CrdStatEnum = UmaVendor.CRD.Model.CrdStatEnum;

namespace UmaVendor.CRD.Ingenico
{
    public class IngenicoCrdConnector 
    {

        #region Constructor

        public IngenicoCrdConnector()
        {
            PosStatus = false;
            m_uniqueId = new byte[24];
        }

        #endregion

        #region Members

        /// <summary>
        /// Pos cihazında yapılan işlemlerin numarası tutulur.
        /// </summary>
        private static ulong ACTIVE_TRX_HANDLE;

        /// <summary>
        /// Posa bağlı olan com portu interfaceid'si tutulur.
        /// </summary>
        private static uint selectedInterface;

        /// <summary>
        /// Pos cihazında işlem olup olmadığını tutar.işlem var ise true yok ise false.
        /// </summary>
        public bool PosStatus { get; set; }
        public bool Connection { get; set; }
        public const string DLL_VERSION_MIN = "1602030800";
        public const int TIMEOUT_DEFAULT = 10000;	// 10 seconds
        public const int TIMEOUT_CARD_TRANSACTIONS = 60000;	// 60 seconds
        public const int TIMEOUT_ECHO = 1000;	// 1 seconds
        public const int TIMEOUT_PRINT_MF = 20000;	// 20 seconds
        public const int TIMEOUT_DATABASE_EXECUTE = 20000;	// 20 seconds
        public const int MAX_UNIQUE_ID = 256;
        public byte[] m_uniqueId;

        #endregion

        #region Methods

        private bool checkDllVersion()
        {
            byte[] dllVersionArr = new byte[24];
            string dllVersion = Encoding.Default.GetString(dllVersionArr);

            var ret = GMPSmartDLL.GMP_GetDllVersion(dllVersionArr);
            if (ret != 0)
            {
                throw new CrdException("Invalid Return Code: " + ret);
            }
            else if (String.Compare(dllVersion, DLL_VERSION_MIN) < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// TODO: Buradan aktif kasiyer ismi,indeksi, ökc versiyonu vb. bilgiler dönüyor,
        /// bunlar için model sınıfları tasarlanıp bu metoddan döndürülmeli.
        /// </summary>
        /// <returns></returns>
        private CrdStatEnum echo()
        {
            ST_ECHO stEcho = new ST_ECHO();

            uint resp = Json_GMPSmartDLL.FP3_Echo(selectedInterface, ref stEcho, 1000);
            if (resp != 0)
            {
                string errorMessage = getDefineText(resp);
                throw new CrdException("Pos Bağlantısı Kesildi." + getDefineText(resp));
            }
            else
            {
                switch (stEcho.ecrMode.ToString())
                {
                    case "0":
                        return CrdStatEnum.ECR_SERVICE_MODE;
                    case "1":
                        return CrdStatEnum.ECR_USER_MODE;
                    case "8":
                        return CrdStatEnum.ECR_BLOCKED_MODE;
                    case "9":
                        return CrdStatEnum.ECR_MAINTENANCE_MODE;
                    case "10":
                        return CrdStatEnum.ECR_DEMO_MODE;
                    default:
                        return CrdStatEnum.ECR_INVALID_MODE;
                }
            }
        }

        private void handshake(uint selectedInterface)
        {

            string date = DateTime.Now.ToString("ddMMyy");
            string time = DateTime.Now.ToString("HHmmss");
            ST_GMP_PAIR pairing = new ST_GMP_PAIR();
            pairing.szExternalDeviceBrand = "INGENICO";
            pairing.szExternalDeviceModel = "IWE280";
            pairing.szExternalDeviceSerialNumber = "12344567";
            pairing.szEcrSerialNumber = "JHWE20000079";
            pairing.szProcOrderNumber = "000001";
            pairing.szProcDate = "030816";
            pairing.szProcTime = "144345";

            ST_GMP_PAIR_RESP pairingResp = new ST_GMP_PAIR_RESP();
            var resp = Json_GMPSmartDLL.FP3_StartPairingInit(selectedInterface, ref pairing, ref pairingResp, Defines.TIMEOUT_DEFAULT);


            byte[] numArray = new byte[256];
            GMPSmartDLL.GetErrorMessage(resp, numArray);
            var message = Encoding.GetEncoding(65001).GetString(numArray);
            if (resp != Defines.TRAN_RESULT_OK)
            {
                throw new CrdException("Handshake fail with error code: " + resp + "." + getDefineText(resp));
            }

            IngenicoCrdConnector.selectedInterface = selectedInterface;





        }

        private string formatAmount(uint amount, ECurrency currency)
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

        /// <summary>
        /// Pos cihazına gidildiğinde pos cihazından dönen handle
        /// </summary>
        /// <param name="hInt">Com portu interfaceid'si</param>
        /// <param name="hTrx">pos cihazından dönen işlem id'si</param>
        private void AddTrxHandles(uint hInt, ulong hTrx)
        {
            ACTIVE_TRX_HANDLE = hTrx;
        }

        private void startTicket()
        {
            uint retcode = Defines.TRAN_RESULT_OK;
            m_uniqueId = new byte[24] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            retcode = GMPSmartDLL.FP3_Start(selectedInterface, ref ACTIVE_TRX_HANDLE, 0, m_uniqueId, m_uniqueId.Length, null, 0, null, 0, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode == Defines.APP_ERR_ALREADY_DONE)
            {
                OnBnClickedButtonVoidAll();
                startTicket();
            }
        }

        private void printHeader(TTicketType headerTicketType)
        {
            uint retcode;
            //if (GMPSmartDLL.FiscalPrinter_GetHandle() == 0)
            //    return;

            retcode = GMPSmartDLL.FP3_TicketHeader(selectedInterface, ACTIVE_TRX_HANDLE, headerTicketType, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode != 0)
            {
                throw new CrdException("Fiş Başlığı Çıkartma Hatası : " + getDefineText(retcode));
            }
        }

        private void optionsFlag()
        {
            UInt64 activeFlags = 0;

            var retcode = GMPSmartDLL.FP3_OptionFlags(selectedInterface, ACTIVE_TRX_HANDLE, ref activeFlags, Defines.GMP3_OPTION_ECHO_PRINTER | Defines.GMP3_OPTION_ECHO_ITEM_DETAILS | Defines.GMP3_OPTION_ECHO_PAYMENT_DETAILS, 0, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode != 0)
            {
                GMPSmartDLL.FP3_Close(selectedInterface, 0, Defines.TIMEOUT_DEFAULT);
                string errorString = getDefineText(retcode);
                throw new CrdException(string.IsNullOrEmpty(errorString) ? "optionsFlag Error: " : errorString);
            }
        }

        public ST_TICKET getTicket()
        {
            ST_TICKET m_stTicket = new ST_TICKET();
            var retcode = Json_GMPSmartDLL.FP3_GetTicket(selectedInterface, ACTIVE_TRX_HANDLE, ref m_stTicket, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode != 0)
            {
                throw new CrdException("optionsFlag Error: " + retcode);
            }
            return m_stTicket;
        }

        private void itemSale(SaleItemDataModel saleItem)
        {
            /**
             * SaleItem'daki final amount her zaman türk lirasıdır :*
             */
            ushort currency = 949;
            uint retcode;
            byte unitType = 0;
            UInt32 itemCount = saleItem.Quantity;
            byte itemCountPrecition = 0;
            ST_TICKET m_stTicket = new ST_TICKET();
            ST_ITEM stItem = new ST_ITEM();

            stItem.type = Defines.ITEM_TYPE_DEPARTMENT;
            stItem.subType = 0;
            stItem.deptIndex = saleItem.DepartmentIndex;
            stItem.amount = Extensions.ConvertToUint(saleItem.FinalAmount);
            stItem.currency = currency;
            stItem.count = itemCount;
            stItem.unitType = unitType;
            stItem.pluPriceIndex = 0;
            stItem.countPrecition = itemCountPrecition;
            stItem.name = saleItem.MaterialName;
            stItem.barcode = saleItem.EAN11;
            retcode = Json_GMPSmartDLL.FP3_ItemSale(selectedInterface, ACTIVE_TRX_HANDLE, ref stItem, ref m_stTicket, TIMEOUT_DEFAULT);


            if (retcode != 0)
            {
                throw new CrdException("Ürün satış  hatası: " + getDefineText(retcode));
            }
        }

        private double subTotal()
        {
            ST_TICKET m_stTicket = new ST_TICKET();

            var retcode = Json_GMPSmartDLL.FP3_Pretotal(0, 0, ref m_stTicket, TIMEOUT_DEFAULT);
            if (retcode != 0)
            {
                throw new CrdException("subTotal error: " + (int)retcode);
            }

            return Extensions.ConvertToDouble(m_stTicket.TotalReceiptAmount);

        }

        private PaymentResultModel cashPayment(uint amount)
        {
            try
            {
                UInt16 currencyOfPayment = (UInt16)ECurrency.CURRENCY_TL;

                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();
                stPaymentRequest.typeOfPayment = (uint)EPaymentTypes.PAYMENT_CASH_TL;
                stPaymentRequest.subtypeOfPayment = 0;
                stPaymentRequest.payAmount = amount;
                stPaymentRequest.payAmountCurrencyCode = currencyOfPayment;

                return getPayment(stPaymentRequest);
            }
            catch (Exception e)
            {
                Logger.Error("CashPayment", e);
                throw e;
            }


        }

        private PaymentResultModel currencyPayment(uint amount, ushort currency)
        {
            try
            {
                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();
                stPaymentRequest.typeOfPayment = (uint)EPaymentTypes.PAYMENT_CASH_CURRENCY;
                stPaymentRequest.subtypeOfPayment = 0;
                stPaymentRequest.payAmount = amount;
                stPaymentRequest.payAmountCurrencyCode = currency;
                return getPayment(stPaymentRequest);
            }
            catch (Exception e)
            {
                Logger.Error("CurrencyPayment", e);
                throw e;
            }

        }

        private PaymentResultModel getPayment(ST_PAYMENT_REQUEST stPaymentRequest)
        {
            try
            {
                PaymentResultModel resultMdl = new PaymentResultModel();
                uint retcode;
                ST_TICKET m_stTicket = new ST_TICKET();
                string display = "";


                retcode = Json_GMPSmartDLL.FP3_Payment(selectedInterface, ACTIVE_TRX_HANDLE, ref stPaymentRequest, ref m_stTicket, 120000);


                if (retcode == Defines.TRAN_RESULT_OK)
                {

                    if (m_stTicket.TotalReceiptAmount == 0)
                        m_stTicket.TotalReceiptAmount = m_stTicket.invoiceAmount;

                    display = String.Format("TOPLAM : {0}", formatAmount(m_stTicket.TotalReceiptAmount, ECurrency.CURRENCY_TL));

                    if (m_stTicket.CashBackAmount != 0)
                        display += String.Format(Environment.NewLine + "P.ÜSTÜ : {0}", formatAmount(m_stTicket.CashBackAmount, ECurrency.CURRENCY_TL));
                    else if (m_stTicket.TotalReceiptAmount != 0)
                        display += String.Format(Environment.NewLine + "KALAN : {0}", formatAmount(m_stTicket.TotalReceiptAmount - m_stTicket.TotalReceiptPayment, ECurrency.CURRENCY_TL));
                    else
                        display += String.Format(Environment.NewLine + "ÖDENEN : {0}", formatAmount(m_stTicket.TotalReceiptPayment, ECurrency.CURRENCY_TL));

                    Console.WriteLine(display);
                    if ((stPaymentRequest.typeOfPayment == (uint)EPaymentTypes.PAYMENT_BANK_CARD) || (stPaymentRequest.typeOfPayment == (uint)EPaymentTypes.PAYMENT_MOBILE))
                    {
                        resultMdl.BankName = m_stTicket.stPayment[0].stBankPayment.bankName;
                        resultMdl.AuthorizeCode = m_stTicket.stPayment[0].stBankPayment.authorizeCode;
                        resultMdl.BankBkmId = m_stTicket.stPayment[0].stBankPayment.bankBkmId;
                        resultMdl.BatchNo = m_stTicket.stPayment[0].stBankPayment.batchNo;
                        resultMdl.Stan = m_stTicket.stPayment[0].stBankPayment.stan;
                        resultMdl.TerminalId = m_stTicket.stPayment[0].stBankPayment.terminalId;
                        resultMdl.NumberOfBonus = m_stTicket.stPayment[0].stBankPayment.numberOfbonus;
                        resultMdl.NumberOfDiscount = m_stTicket.stPayment[0].stBankPayment.numberOfdiscount;
                        resultMdl.MerchantId = m_stTicket.stPayment[0].stBankPayment.merchantId;
                        resultMdl.CardHolderName = m_stTicket.stPayment[0].stBankPayment.stCard.holderName;

                        resultMdl.CardPan = m_stTicket.stPayment[0].stBankPayment.stCard.pan;
                        resultMdl.Display = display;
                    }

                    if (m_stTicket.CashBackAmount != 0)
                        resultMdl.CashBackAmount = Extensions.ConvertToDouble(m_stTicket.CashBackAmount);

                    if (m_stTicket.TotalReceiptPayment != 0)
                        resultMdl.PayAmount = Extensions.ConvertToDouble(m_stTicket.TotalReceiptPayment);

                    if ((m_stTicket.TotalReceiptAmount - m_stTicket.TotalReceiptPayment) != 0)
                        resultMdl.RemainingAmount = Extensions.ConvertToDouble(m_stTicket.TotalReceiptAmount - m_stTicket.TotalReceiptPayment);
                }
                else
                {

                    throw new CrdException("Ödeme Hatası : " + getDefineText(retcode));
                }

                return resultMdl;
            }
            catch (Exception e)
            {
                Logger.Error("Get Payment", e);
                throw e;
            }

        }

        private PaymentResultModel bankPayment(uint amount, ushort currency, ushort bkmId, ushort numberOfInstallments)
        {
            try
            {

                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();

                stPaymentRequest.typeOfPayment = (uint)EPaymentTypes.PAYMENT_BANK_CARD;
                stPaymentRequest.subtypeOfPayment = (uint)EPaymentSubtypes.PAYMENT_SUBTYPE_PROCESS_ON_POS;
                stPaymentRequest.payAmount = amount;
                stPaymentRequest.payAmountCurrencyCode = currency;
                stPaymentRequest.bankBkmId = bkmId;
                stPaymentRequest.numberOfinstallments = numberOfInstallments;

                stPaymentRequest.rawData = Encoding.Default.GetBytes("RawData from external application for the payment application");
                stPaymentRequest.rawDataLen = (ushort)stPaymentRequest.rawData.Length;
                return getPayment(stPaymentRequest);

            }
            catch (Exception e)
            {
                Logger.Error("BankPayment :", e);
                throw e;
            }
        }

        private PaymentResultModel pointPayment(uint amount, ushort currency)
        {
            try
            {

                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();
                stPaymentRequest.typeOfPayment = (uint)PaymentTypeEnum.PAYMENT_PUAN;
                stPaymentRequest.subtypeOfPayment = 0;
                stPaymentRequest.payAmount = amount;
                stPaymentRequest.payAmountCurrencyCode = currency;
                return getPayment(stPaymentRequest);
            }
            catch (Exception e)
            {
                Logger.Error("PointPayment", e);
                throw e;
            }
        }

        private PaymentResultModel giftCardPayment(uint amount, ushort currency)
        {
            try
            {
                ST_PAYMENT_REQUEST stPaymentRequest = new ST_PAYMENT_REQUEST();
                stPaymentRequest.typeOfPayment = (uint)PaymentTypeEnum.PAYMENT_HEDIYE_CEKI;
                stPaymentRequest.subtypeOfPayment = 0;
                stPaymentRequest.payAmount = amount;
                stPaymentRequest.payAmountCurrencyCode = currency;
                return getPayment(stPaymentRequest);
            }
            catch (Exception e)
            {
                Logger.Error("giftCardPayment", e);
                throw;
            }

        }

        private BankPaymentAppInfo[] getBankApplicationInfo()
        {

            byte numberOfTotalRecords = 0;
            byte numberOfTotalRecordsReceived = 0;
            ST_PAYMENT_APPLICATION_INFO[] stPaymentApplicationInfo = new ST_PAYMENT_APPLICATION_INFO[24];

            var retcode = Json_GMPSmartDLL.FP3_GetPaymentApplicationInfo(selectedInterface, ref numberOfTotalRecords, ref numberOfTotalRecordsReceived, ref stPaymentApplicationInfo, 24);


            if (retcode != 0)
            {
                throw new CrdException("getBankApplicationInfo error: " + retcode);
            }
            else if (numberOfTotalRecordsReceived == 0)
            {
                throw new CrdException("getBankApplicationInfo error: Payment app not found");
            }
            else
            {
                BankPaymentAppInfo[] bpAppInfo = new BankPaymentAppInfo[numberOfTotalRecordsReceived];
                for (int i = 0; i < bpAppInfo.Length; i++)
                {
                    bpAppInfo[i] = new BankPaymentAppInfo();
                    bpAppInfo[i].Name = Encoding.Default.GetString(stPaymentApplicationInfo[i].name)
                        .Replace("\0", String.Empty);
                    bpAppInfo[i].BkmId = stPaymentApplicationInfo[i].u16BKMId;
                    bpAppInfo[i].Priority = stPaymentApplicationInfo[i].Priority;
                    bpAppInfo[i].Status = stPaymentApplicationInfo[i].Status;
                }
                return bpAppInfo;
            }
        }

        private void printBeforeMF()
        {
            var retcode = GMPSmartDLL.FP3_PrintBeforeMF(selectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            HandleErrorCode(retcode);
            if (retcode != 0)
            {
                throw new CrdException("printBeforeMF error:" + getDefineText(retcode));

            }
        }

        private void printMF()
        {
            var retcode = GMPSmartDLL.FP3_PrintMF(selectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            if (retcode != 0)
            {
                throw new CrdException("printMF error:" + getDefineText(retcode));
            }
        }

        private void closeHandle()
        {
            Array.Clear(m_uniqueId, 0, m_uniqueId.Length);
            var retcode = GMPSmartDLL.FP3_Close(selectedInterface, ACTIVE_TRX_HANDLE, TIMEOUT_DEFAULT);
            ACTIVE_TRX_HANDLE = 0;
            if (retcode != 0)
            {
                throw new CrdException("closeHandle error:" + getDefineText(retcode));
            }
        }

        private void receiptAmountDecrease(int changedAmount)
        {
            uint retcode;
            ST_TICKET m_stTicket = new ST_TICKET();

            retcode = Json_GMPSmartDLL.FP3_Minus(selectedInterface, 0, changedAmount, "ss", ref m_stTicket, (ushort)(0xFFFF), TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("receiptAmountDecrease error: " + retcode);
            }
        }

        private void receiptAmountIncrease(int changedAmount)
        {
            uint retcode;
            ST_TICKET m_stTicket = new ST_TICKET();

            retcode = Json_GMPSmartDLL.FP3_Plus(selectedInterface, 0, changedAmount, "değişti", ref m_stTicket, (ushort)(0xFFFF), TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("receiptAmountIncrease error: " + retcode);
            }
        }

        private int receiptPercentDecrease(byte percent)
        {
            uint retcode;
            ST_TICKET m_stTicket = new ST_TICKET();
            int changedAmount = 0;
            retcode = Json_GMPSmartDLL.FP3_Dec(selectedInterface, 0, percent, "DEĞ", ref m_stTicket, (ushort)(0xFFFF), ref changedAmount, TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("receiptPercentDecrease error: " + retcode);
            }
            return changedAmount;
        }

        private int receiptPercentIncrease(byte percent)
        {
            uint retcode;
            ST_TICKET m_stTicket = new ST_TICKET();
            int changedAmount = 0;
            retcode = Json_GMPSmartDLL.FP3_Inc(selectedInterface, 0, percent, "asd", ref m_stTicket, (ushort)(0xFFFF), ref changedAmount, TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("receiptPercentIncrease error: " + retcode);
            }
            return changedAmount;
        }

        private void voidItem(ushort itemIndex, ulong itemCount)
        {
            ST_TICKET m_stTicket = new ST_TICKET();
            int retcode = Json_GMPSmartDLL.FP3_VoidItem(selectedInterface, 0, itemIndex, itemCount, 0, ref m_stTicket, TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("itemPercentIncrease error: " + retcode);
            }
        }

        private void voidReceipt()
        {
            ST_TICKET m_stTicket = new ST_TICKET();

            uint retcode = Json_GMPSmartDLL.FP3_VoidAll(selectedInterface, 0, ref m_stTicket, TIMEOUT_DEFAULT);

            if (retcode != 0)
            {
                throw new CrdException("itemPercentIncrease error: " + retcode);
            }
        }

        #endregion

        #region InterfaceImpl

        public CrdStatEnum GetCrdStatus()
        {
            return echo();
        }

        public uint GetInterfaceID(uint InterfaceListIndex, byte[] ID, uint IdLength)
        {
            return GMPSmartDLL.FP3_GetInterfaceID(InterfaceListIndex, ID, IdLength);
        }

        public TicketInfoModel StartInvoice(ST_INVIOCE_INFO stInvioceInfo)
        {
            PosStatus = true;
            startTicket();
            optionsFlag();
            setInvoice(stInvioceInfo);
            printHeader(TTicketType.TInvoice);

            ST_TICKET ticket = getTicket();

            TicketInfoModel ticketInfoMdl = new TicketInfoModel();
            ticketInfoMdl.EJNo = ticket.EJNo;
            ticketInfoMdl.ZNo = ticket.ZNo;
            ticketInfoMdl.ReceiptNo = ticket.FNo;
            ticketInfoMdl.TicketDate = DateTime.Now;
            return ticketInfoMdl;
        }

        private void setInvoice(ST_INVIOCE_INFO stInvioceInfo)
        {

            ST_TICKET stTicket = new ST_TICKET();

            uint retcode = Json_GMPSmartDLL.FP3_SetInvoice(selectedInterface, ACTIVE_TRX_HANDLE, ref stInvioceInfo, ref stTicket, 10000);
            byte[] Buffer = new byte[2000];
            GMPSmartDLL.GetErrorMessage(retcode, Buffer);
            var message = Encoding.GetEncoding(65001).GetString(Buffer);
            if (retcode != 0)
            {
                throw new CrdException(string.Format("Fatura fiş başlığı çıkarma hatası : {0}", getDefineText(retcode)));
            }

        }

        public TicketInfoModel StartReceipt()
        {
            try
            {

                startTicket();
                optionsFlag();
                printHeader(TTicketType.TProcessSale);

                ST_TICKET ticket = getTicket();

                TicketInfoModel ticketInfoMdl = new TicketInfoModel();
                ticketInfoMdl.EJNo = ticket.EJNo;
                ticketInfoMdl.ZNo = ticket.ZNo;
                ticketInfoMdl.ReceiptNo = ticket.FNo;
                ticketInfoMdl.TicketDate = DateTime.Now;
                return ticketInfoMdl;
            }
            catch (Exception e)
            {
                Logger.Error("Fiş Başlatma Hatası : ", e);
                throw e;
            }

        }

        public void ItemSale(SaleItemDataModel saleItem)
        {
            try
            {

                itemSale(saleItem);
            }
            catch (Exception e)
            {

                Logger.Error("ItemSale", e);
                throw e;
            }
        }

        public double Subtotal()
        {
            return subTotal();
        }

        public double SubTotalCondition(ConditionTypeEnum conditionType, double firstAmount, double amount)
        {
            double changedAmount;
            if (conditionType == ConditionTypeEnum.RECEIPT_AMOUNT_DECREASE)
            {
                changedAmount = firstAmount - amount;
                receiptAmountDecrease(Extensions.ConvertToInt(amount));
            }
            else if (conditionType == ConditionTypeEnum.RECEIPT_AMOUNT_INCREASE)
            {
                changedAmount = firstAmount + amount;
                receiptAmountIncrease(Extensions.ConvertToInt(amount));
            }
            else if (conditionType == ConditionTypeEnum.RECEIPT_PERCENT_DECREASE)
            {
                changedAmount = firstAmount - Extensions.ConvertToDouble(receiptPercentDecrease(Convert.ToByte(amount)));
            }
            else if (conditionType == ConditionTypeEnum.RECEIPT_PERCENT_INCREASE)
            {
                changedAmount = firstAmount + Extensions.ConvertToDouble(receiptPercentIncrease(Convert.ToByte(amount)));
            }
            else
            {
                throw new CrdException("SubTotalCondition, unknown condition type error: " + conditionType.ToString());
            }
            return changedAmount;
        }

        public PaymentResultModel GetPayment(SalePaymentDataModel payment)
        {
            try
            {
                PaymentResultModel returnMdl;
                if (payment.PaymentTypeCode == (int)PaymentTypeEnum.PAYMENT_BANK_CARD)
                {
                    returnMdl = bankPayment(Extensions.ConvertToUint(payment.TotalAmount), payment.ExchangeShortCode, payment.BkmId, payment.InstallmentCount);
                }
                else if (payment.PaymentTypeCode == (int)PaymentTypeEnum.PAYMENT_CASH_CURRENCY)
                {
                    returnMdl = currencyPayment(Extensions.ConvertToUint(payment.TotalAmount), payment.ExchangeShortCode);
                }
                else if (payment.PaymentTypeCode == (int)PaymentTypeEnum.PAYMENT_CASH_TL)
                {
                    returnMdl = cashPayment(Extensions.ConvertToUint(payment.TotalAmount));
                }
                else if (payment.PaymentTypeCode == (int)PaymentTypeEnum.PAYMENT_HEDIYE_CEKI)
                {
                    returnMdl = giftCardPayment(Extensions.ConvertToUint(payment.TotalAmount), payment.ExchangeShortCode);
                }
                else if (payment.PaymentTypeCode == (int)PaymentTypeEnum.PAYMENT_PUAN)
                {
                    returnMdl = pointPayment(Extensions.ConvertToUint(payment.TotalAmount), payment.ExchangeShortCode);
                }
                else
                {
                    throw new CrdException("GetPayment Error: unknown payment type code");
                }
                return returnMdl;
            }
            catch (Exception e)
            {
                Logger.Error("GetPayment", e);
                throw e;
            }

        }

        public void HandleErrorCode(UInt32 errorCode)
        {

            if (errorCode == Defines.APP_ERR_GMP3_INVALID_HANDLE)
            {
                ACTIVE_TRX_HANDLE = 0;
                m_uniqueId = new byte[24] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                UInt32 retcode = GMPSmartDLL.FP3_Start(selectedInterface, ref ACTIVE_TRX_HANDLE, 0, m_uniqueId, m_uniqueId.Length, null, 0, null, 0, TIMEOUT_DEFAULT);

                if (retcode == Defines.APP_ERR_ALREADY_DONE)
                    retcode = ReloadTransaction();


            }

            if (errorCode == 2358)
            {
                uint resp = GMPSmartDLL.FP3_Close(selectedInterface, ACTIVE_TRX_HANDLE, Defines.TIMEOUT_DEFAULT);
                ACTIVE_TRX_HANDLE = 0;
            }

        }

        private UInt32 OnBnClickedButtonVoidAll()
        {
            uint RetCode;
            ST_TICKET m_stTicket = new ST_TICKET();
            RetCode = Json_GMPSmartDLL.FP3_VoidAll(selectedInterface, ACTIVE_TRX_HANDLE, ref m_stTicket, Defines.TIMEOUT_DEFAULT);
            if (RetCode != 0)
            {
                HandleErrorCode(RetCode);
                return RetCode;
            }

            uint resp = GMPSmartDLL.FP3_Close(selectedInterface, ACTIVE_TRX_HANDLE, Defines.TIMEOUT_DEFAULT);
            if (RetCode != 0)
            {
                HandleErrorCode(RetCode);
                return RetCode;
            }

            ACTIVE_TRX_HANDLE = 0;
            return RetCode;
        }

        UInt32 ReloadTransaction()
        {
            UInt32 RetCode = 0;
            ST_TICKET m_stTicket = new ST_TICKET();
            UInt64 activeFlags = 0;

            RetCode = GMPSmartDLL.FP3_OptionFlags(selectedInterface, ACTIVE_TRX_HANDLE, ref activeFlags, Defines.GMP3_OPTION_ECHO_PRINTER | Defines.GMP3_OPTION_ECHO_ITEM_DETAILS | Defines.GMP3_OPTION_ECHO_PAYMENT_DETAILS, 0, Defines.TIMEOUT_DEFAULT);
            if (RetCode != Defines.TRAN_RESULT_OK)
                return RetCode;

            RetCode = Json_GMPSmartDLL.FP3_GetTicket(selectedInterface, ACTIVE_TRX_HANDLE, ref m_stTicket, Defines.TIMEOUT_DEFAULT);
            if (RetCode != Defines.TRAN_RESULT_OK)
                return RetCode;

            return RetCode;
        }

        public void CloseReceipt()
        {
            try
            {

                printBeforeMF();
                printMF();
                closeHandle();
                PosStatus = false;
            }
            catch (Exception e)
            {
                Logger.Error("CloseReceipt", e);
                throw e;
            }
        }

        public uint GetInterfaceXmlDataByHandle(uint CurrentInterface, ref ST_INTERFACE_XML_DATA stInterfaceXmlData)
        {
            byte[] szJsonOut = new byte[Defines.STANDART_BUFFER];

            UInt32 retcode = Json_GMPSmartDLL.Json_FP3_GetInterfaceXmlDataByHandle(CurrentInterface, szJsonOut, szJsonOut.Length);

            if (retcode == 0)
            {
                string retJsonString = GMP_Tools.SetEncoding(szJsonOut);
                stInterfaceXmlData = JsonConvert.DeserializeObject<ST_INTERFACE_XML_DATA>(retJsonString);
            }
            return retcode;
        }

        public List<ST_TAX_RATE> GetTaxRate()
        {
            ST_TAX_RATE[] stTaxRates = new ST_TAX_RATE[8];

            byte indexOfTaxRates = 0;
            byte indexOfDepartments = 0;
            int numberOfTotalTaxRates = 0;

            int numberOfTotalRecordsReceived = 0;
            uint retcode = Json_GMPSmartDLL.FP3_GetTaxRates_Ex(selectedInterface, indexOfTaxRates, ref numberOfTotalTaxRates, ref numberOfTotalRecordsReceived, ref stTaxRates, 8 - indexOfTaxRates);


            for (int i = 0; i < stTaxRates.Length; i++)
            {

                stTaxRates[i].taxRate = (ushort)(stTaxRates[i].taxRate / 100);

            }

            return stTaxRates.ToList();
        }

        public void Handshake(uint selectedInterface)
        {
            try
            {
                PosStatus = true;
                handshake(selectedInterface);
                PosStatus = false;
            }
            catch (Exception e)
            {
                PosStatus = false;
                Logger.Error("Handshake", e);
                throw e;
            }

        }

        public void VoidItem(SaleItemDataModel saleItem, uint quantity)
        {
            try
            {
                voidItem(saleItem.TicketIndex, quantity);

            }
            catch (Exception e)
            {
                Logger.Error("Void Item", e);
                throw e;
            }
        }

        public void VoidReceipt()
        {
            voidReceipt();
        }

        public BankPaymentAppInfo[] GetBankPaymentAppInfo()
        {
            return getBankApplicationInfo();
        }

        private string getDefineText(uint code)
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

        /// <summary>
        /// Pos cihazında işlem yapılıp yapılmadığını kontrol eder.
        /// </summary>
        /// <returns>True pos cihazında işlem var false :  pos cihazı boşta</returns>
        public bool CheckCrdProcess()
        {
            return PosStatus;
        }

        public bool CheckConnection()
        {
            return Connection;
        }

        private UInt64 GetTransactionHandle(UInt32 InterfaceHandle)
        {
            return ACTIVE_TRX_HANDLE;
        }

        public uint GetInterfaceHandleList(ref uint[] InterfaceList, uint InterfaceListLength)
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

        #endregion InterfaceImpl

    }
}
