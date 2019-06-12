using System.Net;

namespace DynamicPos.CrdData.Model
{

    /// <summary>
    /// Fiş başlangıç geri dönüşlerini tutar.
    /// </summary>
    public class DocumentHeaderResultModel
    {

        #region Member

        /// <summary>
        /// Fiş Z numarasını tutar.
        /// </summary>
        public string ZNum { get; set; }

        /// <summary>
        /// Fiş Numarasını tutar.
        /// </summary>
        public string ReceiptNum { get; set; }

        /// <summary>
        /// Fiş İşlem Tarihini tutar.
        /// </summary>
        public string TranDate { get; set; }

        /// <summary>
        /// Fiş İşlem Saatini tutar.
        /// </summary>
        public string TranTime { get; set; }

        /// <summary>
        /// İşlem durum mesajını belirtir.
        /// </summary>
        public string ProcessMessage { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        #endregion

    }
}
