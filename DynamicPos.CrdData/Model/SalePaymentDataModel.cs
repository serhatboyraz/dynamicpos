

namespace DynamicPos.CrdData.Model
{

    /// <summary>
    /// Satışa ait ödemelerin tutulduğu sınıf.
    /// </summary>
    public class SalePaymentDataModel
    {

        #region Properties

        /// <summary>
        /// Ödeme Türü Kodu
        /// 01 : Nakit
        /// 02 : Kredi Kartı
        /// </summary>
        public string PaymentTypeCode { get; set; }

        /// <summary>
        /// İşlem tipini belirtir.
        /// </summary>
        public string ProcessTypeCode { get; set; }

        /// <summary>
        /// Kur çevrimi default ödeme türüne göre yapıldıktan sonraki ödeme toplamı
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Para Birimi indexi
        /// </summary>
        public string ExchangeCodeIndex { get; set; }


        public string InstallmentCnt { get; set; }
        #endregion

    }
}
