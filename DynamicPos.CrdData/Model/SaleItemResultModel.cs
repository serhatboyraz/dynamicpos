using System.Collections.Generic;
using System.Net;

namespace DynamicPos.CrdData.Model
{

    /// <summary>
    /// Satılan ürün geri dönüş modeli
    /// </summary>
    public class SaleItemResultModel
    {

        #region Properties

        public string ZNum { get; set; }

        public string ReceiptNo { get; set; }

        public List<TransItemsResult> TransItemsResult { get; set; }

        #endregion

    }

    /// <summary>
    /// Satışı yapılan ürün modeli
    /// </summary>
    public class TransItemsResult
    {

        /// <summary>
        /// Ürün adı
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// Ürün satış durumu HttpStatusCode olarak belirtilir
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// İşlem durum mesajı
        /// </summary>
        public string ProcessMessage { get; set; }

    }
}
