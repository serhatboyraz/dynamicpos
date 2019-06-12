using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class CancelPaymentResultModel
    {
        #region Properties

        /// <summary>
        /// İşlem başarı durumunu belirtir.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Fişte ödenecek toplam fiyatı belirtir.
        /// </summary>
        public string TotalAmount { get; set; }

        public string ProcessMessage { get; set; }

        #endregion
    }
}
