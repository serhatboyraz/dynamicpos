using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class SumAmountResultModel
    {

        #region Properties

        /// <summary>
        /// Toplam ücret
        /// </summary>
        public string SumAmount { get; set; }

        /// <summary>
        /// Http Durumunu tutar.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        #endregion

    }
}
