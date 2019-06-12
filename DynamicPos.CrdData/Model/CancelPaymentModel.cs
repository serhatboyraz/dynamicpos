namespace DynamicPos.CrdData.Model
{
    public class CancelPaymentModel
    {
        #region Properties

        public string PaymentType { get; set; }

        public string Amount { get; set; }

        /// <summary>
        /// BKM-ID EFT Kodu (Length: 2)
        /// </summary>
        public string AcquirerId { get; set; }

        /// <summary>
        /// Yığın numarası (Length: 3)
        /// </summary>
        public string BatchNum { get; set; }

        /// <summary>
        /// İşlem numarası (Length: 3)
        /// </summary>
        public string StanNum { get; set; }

        public string InstallmentCnt { get; set; }

        #endregion
    }
}
