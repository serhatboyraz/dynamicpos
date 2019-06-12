using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using DynamicPos.CrdData.Model;
using DynamicPos.CrdService.Interface;
using DynamicPos.Utils.Helpers;
using Newtonsoft.Json;
using PCPOSOKC;

namespace DynamicPos.Olivetti.Connector
{

    /// <summary>
    /// Olivetti pos cihazı işlemlerinin yapılacağı sınıf.
    /// </summary>
    public class OlivettiConnector : ICrdConnector, IDisposable
    {

        #region Member

        private EcrInterface ecrInterface;


        #endregion

        #region Methods

        public void GetDailyZReport()
        {
            Members rspMem = new Members();
            ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, new Members()
            {
                ReportType = String.Format("{0:x3}", (int)ECR_RPRT_TYPS.Z)
            }, ref rspMem);

        }

        public void GetDailyXReport()
        {
            Members rspMem = new Members();
            ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, new Members()
            {
                ReportType = String.Format("{0:x3}", (int)ECR_RPRT_TYPS.X)
            }, ref rspMem);

        }

        public HandShakeResultModel HandShake(HandShakeModel handShakeModel)
        {
            ecrInterface = new EcrInterface();

            HandShakeResultModel handShakeResultModel = new HandShakeResultModel();
            Members rspMem = new Members();

            ECR_ERRORS openPortStatus;
            switch ((COMMTYPE)Enum.Parse(typeof(COMMTYPE), handShakeModel.PosConnectionType))
            {
                case COMMTYPE.TCPIP:
                    var comc = new CommMedia()
                    {
                        strServerAddr = handShakeModel.PosIp,
                        iServerPort = int.Parse(handShakeModel.PosIpPort)
                    };
                    openPortStatus = (ECR_ERRORS)ecrInterface.COMM_Open(COMMTYPE.TCPIP, comc);
                    break;
                default:
                    openPortStatus = (ECR_ERRORS)ecrInterface.COMM_Open(COMMTYPE.RS232, new CommMedia()
                    {
                        strSerialPort = handShakeModel.ComPort
                    });
                    break;
            }
            //USB Port Bağlantı işlemi

            Logger.Info(string.Format("ConnectStatus  :{0}", openPortStatus));


            if (openPortStatus != ECR_ERRORS.ERR_OK)
            {
                handShakeResultModel.HttpStatusCode = HttpStatusCode.InternalServerError;
                return handShakeResultModel;
            }

            bool encryptedMessage = handShakeModel.EncDisable != "1";
            ecrInterface.SetEcryptedMsgStat(encryptedMessage);

            //Pair işlemi
            try
            {
                ECR_ERRORS pairStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_GMP3Pair, new Members(), ref rspMem);
            }
            catch (Exception ex)
            {
                handShakeResultModel.HttpStatusCode = HttpStatusCode.InternalServerError;
                Logger.Error("PairingException", ex);
                return handShakeResultModel;
            }

            UInt64 ecrConfig = 0x0000000000000000;
            ecrConfig |= 0x1000;

            var ecrConfigParams = new Members();
            ECR_ERRORS setEcrParamResult = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_EcrConfig, new Members()
            {
                EcrConfig = string.Format("{0:X16}", ecrConfig)
            }, ref ecrConfigParams);

            //Kasiyer Girişi
            ECR_ERRORS cashierLoginStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_CashierLogin, new Members()
            {
                CashierId = handShakeModel.CashierId,
                CashierPwd = handShakeModel.CashierPassword,

            }, ref rspMem);

            if (cashierLoginStatus != ECR_ERRORS.ERR_OK)
            {
                handShakeResultModel.HttpStatusCode = HttpStatusCode.InternalServerError;

                return handShakeResultModel;
            }

            //EcrMode Set
            ECR_ERRORS changeEcrModeStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_ChangeEcrMode, new Members()
            {
                EcrMode = handShakeModel.EcrMode
            }, ref rspMem);

            if (changeEcrModeStatus != ECR_ERRORS.ERR_OK)
            {
                handShakeResultModel.HttpStatusCode = HttpStatusCode.InternalServerError;

                return handShakeResultModel;
            }

            handShakeResultModel.HttpStatusCode = HttpStatusCode.OK;

            return handShakeResultModel;
        }

        /// <summary>
        /// Pos Cihazı bağlantı durumunu belirtir.
        /// </summary>
        /// <returns>ConnectionResultModel Bağlantı Durumu</returns>
        public ConnectionResultModel IsConnected()
        {
            ConnectionResultModel connectionResult = new ConnectionResultModel();
            Members rspMem = new Members();

            ecrInterface.SendCmd2ECR(Tags.msgREQ_Ping, new Members(), ref rspMem);
            string a = EcrInterface.GetTranStatusExplain(Convert.ToInt32(rspMem.TranStatus));

            if (rspMem.InternalErrNum == "0")
                connectionResult.StatusCode = HttpStatusCode.OK;
            else
                connectionResult.StatusCode = HttpStatusCode.BadRequest;

            connectionResult.Message = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));

            return connectionResult;
        }

        /// <summary>
        /// Fiş veya fatura başlatmak için kullanılır.
        /// Öncesinde Handshake işlemi uygulanmalı.
        /// </summary>
        public DocumentHeaderResultModel ReceiptBegin(DocumentHeaderModel documentHeader)
        {
            Members rspMem = new Members();
            DocumentHeaderResultModel receiptBeginResult = new DocumentHeaderResultModel();

            ECR_ERRORS receiptBeginStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_RcptBegin, new Members()
            {
                DocumentType = documentHeader.DocType.PadLeft(2, '0'),
                Tckn = documentHeader.CustomerTcNo,
                Vkn = documentHeader.CustomerVkNo,
            }, ref rspMem);

            if (rspMem.InternalErrNum == "0")
            {
                receiptBeginResult.ZNum = rspMem.ZNum;
                receiptBeginResult.ReceiptNum = rspMem.ReceiptNum;
                receiptBeginResult.TranDate = rspMem.TranDate;
                receiptBeginResult.TranTime = rspMem.TranTime;
                receiptBeginResult.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                receiptBeginResult.StatusCode = HttpStatusCode.BadRequest;
            }

            receiptBeginResult.ProcessMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));

            return receiptBeginResult;
        }

        /// <summary>
        /// Do Transaction işleminin gerçekleştiği Metot
        /// </summary>
        /// <param name="saleItem"></param>
        /// <returns></returns>
        public SaleItemResultModel ItemSale(SaleItemDataModel saleItemModel)
        {
            SaleItemResultModel saleItemResult = new SaleItemResultModel();
            saleItemResult.TransItemsResult = new List<TransItemsResult>();
            Members rspMem = new Members();

            if (saleItemModel.TransItems == null)
                return new SaleItemResultModel();

            foreach (var item in saleItemModel.TransItems)
            {
                ECR_ERRORS doTransactionStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_DoTran, new Members()
                {
                    ProcessType = "A1", //departman satış
                    DepartmentId = saleItemModel.DepartmentIndex.PadLeft(2, '0'),
                    Quantity = item.Quantity?.PadLeft(8, '0'),
                    ItemName = item.MaterialName,
                    Amount = item.FinalAmount?.PadLeft((int)FIELD_WIDTH.AMOUNT, '0'),
                    UnitPrice = item.UnitPrice?.PadLeft((int)FIELD_WIDTH.AMOUNT, '0'),
                }, ref rspMem);

                if (rspMem.InternalErrNum == "0")
                {
                    saleItemResult.ZNum = rspMem.ZNum;
                    saleItemResult.ReceiptNo = rspMem.ReceiptNum;
                    saleItemResult.TransItemsResult.Add(new TransItemsResult()
                    {
                        MaterialName = item.MaterialName,
                        HttpStatusCode = HttpStatusCode.OK,
                        ProcessMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum))
                    });
                }
                else
                {
                    saleItemResult.TransItemsResult.Add(new TransItemsResult()
                    {
                        MaterialName = item.MaterialName,
                        HttpStatusCode = HttpStatusCode.BadRequest,
                        ProcessMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum))
                    });
                }
            }

            return saleItemResult;
        }

        /// <summary>
        /// Ödeme yapar.
        /// </summary>
        /// <param name="paymentDataModel"></param>
        /// <returns></returns>
        public GetPaymentResultModel GetPayment(SalePaymentDataModel paymentDataModel)
        {
            GetPaymentResultModel getPaymentResultModel = new GetPaymentResultModel();
            Members rspMem = new Members();
            //ödeme requesti
            Members tranMem = new Members
            {
                ProcessType = paymentDataModel?.ProcessTypeCode?.PadLeft(2, '0'),
                PaymentType = paymentDataModel?.PaymentTypeCode.PadLeft(2, '0'),
                Amount = paymentDataModel?.Amount.PadLeft((int)FIELD_WIDTH.AMOUNT, '0'),
                CurrIndex = paymentDataModel?.ExchangeCodeIndex?.PadLeft(2, '0'),
                InstallmentCnt = paymentDataModel?.InstallmentCnt?.PadLeft(2, '0')
            };


            //ödeme işlemi
            ECR_ERRORS paymentStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_DoPayment, tranMem, ref rspMem);

            if (rspMem.InternalErrNum == "0")
            {

                getPaymentResultModel.AcquirerId = rspMem.AcquirerId;
                getPaymentResultModel.Amount = rspMem.Amount;
                getPaymentResultModel.InstallmentCnt = rspMem.InstallmentCnt;
                getPaymentResultModel.AuthCode = rspMem.AuthCode;
                getPaymentResultModel.BatchNum = rspMem.BatchNum;
                getPaymentResultModel.CardNum = rspMem.CardNum;
                getPaymentResultModel.CardType = rspMem.CardType;
                getPaymentResultModel.IssuerId = rspMem.IssuerId;
                getPaymentResultModel.MerchantId = rspMem.MerchantId;
                getPaymentResultModel.ProcessType = rspMem.ProcessType;
                getPaymentResultModel.StanNum = rspMem.StanNum;
                getPaymentResultModel.TerminalId = rspMem.TerminalId;
                getPaymentResultModel.TranDate = rspMem.TranDate;
                getPaymentResultModel.TranTime = rspMem.TranTime;
                getPaymentResultModel.HttpStatusCode = HttpStatusCode.OK;
                getPaymentResultModel.ProcessMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));
            }
            else
            {
                Logger.Info(JsonConvert.SerializeObject(rspMem));
                getPaymentResultModel.HttpStatusCode = HttpStatusCode.BadRequest;
                getPaymentResultModel.ProcessMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));
            }

            return getPaymentResultModel;
        }


        /// <summary>
        /// Ödeme işlemini iptal eder
        /// </summary>
        /// <param name="cancelPaymentModel"></param>
        /// <returns></returns>
        public CancelPaymentResultModel CancelPayment(CancelPaymentModel cancelPaymentModel)
        {
            Members rspMem = new Members();
            CancelPaymentResultModel cancelPaymentResultModel = new CancelPaymentResultModel();
            Members rspPayment = new Members()
            {
                PaymentType = cancelPaymentModel.PaymentType.PadLeft(2, '0'),
                Amount = cancelPaymentModel.Amount.PadLeft((int)FIELD_WIDTH.AMOUNT, '0'),
                ProcessType = "30",
                AcquirerId = cancelPaymentModel?.AcquirerId?.PadLeft(4, '0'),
                BatchNum = cancelPaymentModel?.BatchNum?.PadLeft(6, '0'),
                StanNum = cancelPaymentModel?.StanNum?.PadLeft(6, '0'),
                InstallmentCnt = cancelPaymentModel?.InstallmentCnt?.PadLeft(2, '0')
            };

            ECR_ERRORS cancelPaymentStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_DoPayment, rspPayment, ref rspMem);

            bool approveResult = (rspMem.ResponseCode == "00" || rspMem.ResponseCode == "08" || rspMem.ResponseCode == "11")
                ? true
                : false;

            if (rspMem.InternalErrNum == "0" && approveResult)
            {
                cancelPaymentResultModel.HttpStatusCode = HttpStatusCode.OK;
                cancelPaymentResultModel.TotalAmount = rspMem.Amount;
            }
            else
            {
                cancelPaymentResultModel.HttpStatusCode = HttpStatusCode.BadRequest;
            }

            cancelPaymentResultModel.ProcessMessage = EcrInterface.GetErrorExplain(int.Parse(rspMem.InternalErrNum));

            return cancelPaymentResultModel;
        }

        /// <summary>
        /// İçerisinde satış olan fiş veya fatura sonlandırmak için kullanılır. 
        /// </summary>
        /// <returns></returns>
        public EndReceiptResultModel EndReceipt()
        {
            EndReceiptResultModel endReceiptResultModel = new EndReceiptResultModel();
            Members rspMem = new Members();
            ECR_ERRORS receiptCloseStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_ReceiptEnd, new Members(), ref rspMem);

            if (rspMem.InternalErrNum == "0")
            {
                endReceiptResultModel.HttpStatusCode = HttpStatusCode.OK;

            }
            else
            {

                endReceiptResultModel.HttpStatusCode = HttpStatusCode.BadRequest;
            }

            endReceiptResultModel.Message = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));

            return endReceiptResultModel;
        }

        /// <summary>
        /// Fiş iptal işlemi yapar.
        /// </summary>
        /// <returns></returns>
        public CancelDocumentResultModel CancelDocument()
        {
            Members rspMem = new Members();
            CancelDocumentResultModel cancelResult = new CancelDocumentResultModel();

            ECR_ERRORS cancelDocumentStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_DoTran, new Members()
            {
                ProcessType = "A7",
            }, ref rspMem);

            if (rspMem.InternalErrNum == "0")
                cancelResult.HttpStatusCode = HttpStatusCode.OK;
            else
            {
                cancelResult.HttpStatusCode = HttpStatusCode.BadRequest;
                Logger.Info("CancelReceipt 400");
            }

            cancelResult.Message = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));

            return cancelResult;
        }

        /// <summary>
        /// Toplam ücreti getirir.
        /// </summary>
        /// <returns></returns>
        public SumAmountResultModel GetSumAmount()
        {
            Members rspMem = new Members();
            SumAmountResultModel sumAmountResult = new SumAmountResultModel();

            ECR_ERRORS sumResult = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_GetReceiptTot, new Members()
            {
                PaymentSummary = new PaymentSummaryTable[20],
                CreditPaymentResult = new CreditPaymentResultTable[20],
                VATGrpTotal = new VATGroupTotalTable[8]
            }, ref rspMem);

            if (rspMem.InternalErrNum == "0")
            {
                sumAmountResult.SumAmount = rspMem.Amount;
                sumAmountResult.HttpStatusCode = HttpStatusCode.OK;
            }
            else
            {
                sumAmountResult.HttpStatusCode = HttpStatusCode.BadRequest;
            }

            return sumAmountResult;
        }

        public void ChangeMode(string mode)
        {
            Members rspMem = new Members();

            ECR_ERRORS changeEcrModeStatus = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_ChangeEcrMode, new Members()
            {
                EcrMode = mode
            }, ref rspMem);

            if (changeEcrModeStatus != ECR_ERRORS.ERR_OK)
            {

            }

        }

        public DoAllProcessResultModel DoAllProcess(DoAllProcessModel allProcessModel)
        {
            DoAllProcessResultModel resultModel = new DoAllProcessResultModel();

            resultModel.StatusCode = HttpStatusCode.BadRequest;

            //ConnectionState İşlemi
            ConnectionResultModel connectionResult = IsConnected();

            resultModel.ConnectionResult = connectionResult;

            resultModel.ProcessStep = "ConnectionResult";
            if (!connectionResult.Message.Contains("Başarılı") && connectionResult.StatusCode != HttpStatusCode.OK)
            {
                return resultModel;
            }

            ChangeMode("02");

            //Fiş Başlatma işlemi

            DocumentHeaderResultModel startReceiptResult = ReceiptBegin(allProcessModel.StartReceiptDataModel);

            resultModel.StartReceiptResult = startReceiptResult;

            resultModel.ProcessStep = "StartReceiptResult";
            if (!startReceiptResult.ProcessMessage.Contains("Başarılı") || startReceiptResult.ReceiptNum == null ||
                startReceiptResult.ZNum == null)
                return resultModel;


            //Ürün yazdırma-satma işlemi

            SaleItemResultModel saleItemResult = ItemSale(allProcessModel.SaleItemDataModel);

            resultModel.SaleItemResult = saleItemResult;

            resultModel.ProcessStep = "SaleItemResult";

            //Toplam ödenecek fiyat

            SumAmountResultModel sumAmountResult = GetSumAmount();

            if (sumAmountResult.SumAmount == null)
                return resultModel;

            //TODO:Bu kısım doğru mu?(kontrol işlemi sağlıklı mı?)
            string sumAmount = allProcessModel.SalePaymentDataModel.Amount.PadLeft(12, '0');
            if (sumAmount.Equals("".PadLeft(12, '0')))
                allProcessModel.SalePaymentDataModel.Amount = sumAmountResult.SumAmount;


            //Ödeme işlemi

            GetPaymentResultModel paymentResult = GetPayment(allProcessModel.SalePaymentDataModel);

            resultModel.SalePaymentResult = paymentResult;

            resultModel.ProcessStep = "SalePaymentResult";

            if (paymentResult.HttpStatusCode != HttpStatusCode.OK)
            {
                return resultModel;
            }

            EndReceiptResultModel close = EndReceipt();
            resultModel.EndReceiptResult = close;

            resultModel.ProcessStep = "EndReceiptResult";

            if (resultModel.EndReceiptResult.HttpStatusCode != HttpStatusCode.OK)
            {
                return resultModel;
            }

            resultModel.StatusCode = HttpStatusCode.OK;
            return resultModel;
        }

        /// <summary>
        /// Dispose işlemini gerçekleştirir.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        public PaymentForCardInfoResultModel PaymentCardInfo(PaymentForCardInfo paymentForCardInfo)
        {
            Members rspMem = new Members();
            Members reqMem = new Members();

            PaymentForCardInfoResultModel resultModel = new PaymentForCardInfoResultModel();
            ECR_ERRORS sumResult = (ECR_ERRORS)ecrInterface.SendCmd2ECR(Tags.msgREQ_InfoInquiry, reqMem, ref rspMem);

            bool approveResult = rspMem.ResponseCode == "00" || rspMem.ResponseCode == "08" || rspMem.ResponseCode == "11" ? true : false;

            if (rspMem.InternalErrNum == "0" && approveResult)
            {

                resultModel.AcquirerId = rspMem.AcquirerId;
                resultModel.Amount = rspMem.Amount;
                resultModel.InstallmentCnt = rspMem.InstallmentCnt;
                resultModel.AuthCode = rspMem.AuthCode;
                resultModel.BatchNum = rspMem.BatchNum;
                resultModel.CardNum = rspMem.CardNum;
                resultModel.CardType = rspMem.CardType;
                resultModel.IssuerId = rspMem.IssuerId;
                resultModel.MerchantId = rspMem.MerchantId;
                resultModel.ProcessType = rspMem.ProcessType;
                resultModel.StanNum = rspMem.StanNum;
                resultModel.TerminalId = rspMem.TerminalId;
                resultModel.TranDate = rspMem.TranDate;
                resultModel.TranTime = rspMem.TranTime;
                resultModel.HttpStatusCode = HttpStatusCode.OK;
                resultModel.StatusMessage = EcrInterface.GetErrorExplain(Convert.ToInt32(rspMem.InternalErrNum));
            }
            else
            {
                resultModel.HttpStatusCode = HttpStatusCode.BadRequest;
                resultModel.StatusMessage = "Başarısız";
            }

            return resultModel;
        }

        #endregion

    }
}
