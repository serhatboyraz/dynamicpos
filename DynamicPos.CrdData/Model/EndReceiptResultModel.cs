using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class EndReceiptResultModel
    {

        #region Member

        public HttpStatusCode HttpStatusCode { get; set; }

        public string Message { get; set; }

        #endregion

    }
}
