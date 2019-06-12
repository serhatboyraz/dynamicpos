using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using DynamicPos.CrdData.Model;
using DynamicPos.CrdService.Interface;
using DynamicPos.Utils.Helpers;
using Newtonsoft.Json;
using PCPOSOKC;

namespace DynamicPos.Hugin.Connector
{

    /// <summary>
    /// Olivetti pos cihazı işlemlerinin yapılacağı sınıf.
    /// </summary>
    public class HuginConnector : ICrdConnector, IDisposable
    {
        public DoAllProcessResultModel DoAllProcess(DoAllProcessModel allProcessModel)
        {
            return null;
        }

        public ConnectionResultModel IsConnected()
        {
            return null;
        }

        public HandShakeResultModel HandShake(HandShakeModel handShakeModel)
        {
            return null;
        }

        public DocumentHeaderResultModel ReceiptBegin(DocumentHeaderModel headerModel)
        {
            return null;
        }

        public SaleItemResultModel ItemSale(SaleItemDataModel saleItemModel)
        {
            return null;
        }

        public GetPaymentResultModel GetPayment(SalePaymentDataModel payment)
        {
            return null;
        }

        public CancelPaymentResultModel CancelPayment(CancelPaymentModel cancelPaymentModel)
        {
            return null;
        }

        public EndReceiptResultModel EndReceipt()
        {
            return null;
        }

        public CancelDocumentResultModel CancelDocument()
        {
            return null;
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

        public void Dispose()
        {
        }
    }
}
