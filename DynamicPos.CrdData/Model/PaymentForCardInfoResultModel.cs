using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class PaymentForCardInfoResultModel
    {
        public string AcquirerId { get; set; }

        public string Amount { get; set; }

        public string InstallmentCnt { get; set; }

        public string AuthCode { get; set; }

        public string BatchNum { get; set; }

        public string CardNum { get; set; }

        public string CardType { get; set; }

        public string IssuerId { get; set; }

        public string MerchantId { get; set; }

        public string ProcessType { get; set; }

        public string StanNum { get; set; }

        public string TerminalId { get; set; }

        public string TranDate { get; set; }

        public string TranTime { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string StatusMessage { get; set; }
    }
}
