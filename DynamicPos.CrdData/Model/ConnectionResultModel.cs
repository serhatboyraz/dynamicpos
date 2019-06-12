using System.Net;

namespace DynamicPos.CrdData.Model
{
    public class ConnectionResultModel
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }
    }
}
