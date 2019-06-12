using System.Net;
using System.Text;
using Newtonsoft.Json;
using Unosquare.Labs.EmbedIO;

namespace DynamicPos.WebServer.Helper
{
   public static class Extensions
   {

        public static void SetRespose(this IHttpResponse httpResponse , HttpStatusCode responseCode, object resultObj)
        {
            httpResponse.AddHeader("Content-Type", "application/json");
            httpResponse.StatusCode = (int)responseCode;
            var outgoingData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resultObj));
            httpResponse.OutputStream.Write(outgoingData, 0, outgoingData.Length);
        }
        public static void SetRespose(this IHttpResponse httpResponse , HttpStatusCode responseCode)
        {
            httpResponse.AddHeader("Content-Type", "application/json");
            httpResponse.StatusCode = (int)responseCode;
        }
    }
}
