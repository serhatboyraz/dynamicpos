using System;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DynamicPos.Utils.Helpers;
using DynamicPos.WebServer.Controller;
using Newtonsoft.Json;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

namespace DynamicPos.WebServer.Helper
{

    /// <summary>
    /// Server Tanımlama, Yönetme Class'ı
    /// </summary>
    public class ServerInit
    {

        #region Member

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Webserver nesnesi
        /// </summary>
        private static Unosquare.Labs.EmbedIO.WebServer webServer;

        #endregion

        #region Properties

        /// <summary>
        /// Url Prefixes'ı tutar.
        /// </summary>
        public string UrlPrefixes;

        /// <summary>
        /// Server bağlantı durumunu tutar.
        /// </summary>
        public bool IsConnected = false;

        #endregion

        #region Methods

        /// <summary>
        /// Serveri açar.
        /// </summary>
        public async Task<string> Start()
        {
            string urlPrefix = ConfigurationSettings.AppSettings.Get("ApiPrefixes");

            try
            {
                string[] urlPrefixParts = urlPrefix.Split(',');
                UrlPrefixes = urlPrefixParts[0];

                webServer = new Unosquare.Labs.EmbedIO.WebServer(urlPrefixParts, RoutingStrategy.Regex);
                webServer.EnableCors();

                webServer.RegisterModule(new WebApiModule());
                webServer.Module<WebApiModule>().RegisterController<PosController>();

                webServer.OnNotFound = ctx =>
                {
                    string requestedPage = string.Format("Page Not Found ! \nUrl : {0}\nBody : {1}\nHeaders : {2}", ctx.Request.Url, ctx.RequestBody(),
                    JsonConvert.SerializeObject(ctx.Request.Headers));
                    Logger.Info(requestedPage);

                    return ctx.HtmlResponseAsync("<center><h1>404</h1><center>");
                };

                cancellationTokenSource = new CancellationTokenSource();
                webServer.RunAsync(cancellationTokenSource.Token);
                              
                IsConnected = true;

                return JsonConvert.SerializeObject(String.Format("{0}", HttpStatusCode.OK));
            }
            catch (Exception e)
            {
                Logger.Error("ServiceInitError", e);

                return JsonConvert.SerializeObject(String.Format("{0}", HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Serveri kapatır.
        /// </summary>
        public void Stop()
        {
            try
            {
                cancellationTokenSource.Cancel();
                webServer.Dispose();
                IsConnected = false;
            }
            catch (Exception e)
            {
               Logger.Error("cancellationTokenSource.CancelException", e);
            }
        }

        #endregion

    }
}
