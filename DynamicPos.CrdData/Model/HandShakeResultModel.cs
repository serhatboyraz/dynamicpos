using System.Net;

namespace DynamicPos.CrdData.Model
{
    /// <summary>
    /// HandShake Geri Dönüş Bilgisini tutan model.
    /// </summary>
    public class HandShakeResultModel
    {
        #region Member

        /// <summary>
        /// Http Status durumunu tutar.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        #endregion

    }
}
