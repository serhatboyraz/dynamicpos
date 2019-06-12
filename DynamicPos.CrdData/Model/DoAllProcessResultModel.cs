using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class DoAllProcessResultModel
    {

        public HttpStatusCode StatusCode { get; set; }

        public string ProcessStep { get; set; }

        public ConnectionResultModel ConnectionResult { get; set; }

        public DocumentHeaderResultModel StartReceiptResult { get; set; }

        public SaleItemResultModel SaleItemResult { get; set; }

        public GetPaymentResultModel SalePaymentResult { get; set; }

        public EndReceiptResultModel EndReceiptResult { get; set; }

    }
}
