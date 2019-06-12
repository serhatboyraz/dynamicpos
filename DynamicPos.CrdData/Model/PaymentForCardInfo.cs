namespace DynamicPos.CrdData.Model
{
    public class PaymentForCardInfo
    {
        public string CardPrefix { get; set; }

        public string AcquirerId { get; set; }

        public string IssuerId { get; set; }

        public string Amount { get; set; }

        public string InstallmentCnt { get; set; }
    }
}
