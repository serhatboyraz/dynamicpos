using System;
using System.Configuration;
using System.Management.Instrumentation;
using DynamicPos.CrdService.Helper;
using DynamicPos.Hugin.Connector;
using DynamicPos.Ingenico.Connector;
using DynamicPos.Olivetti.Connector;

namespace DynamicPos.WebServer.Helper
{

    /// <summary>
    /// Server Yardımcı Class'ı
    /// </summary>
    public static class ServerHelper
    {

        #region Member

        /// <summary>
        /// Server işlemlerinin gerçekleştiren sınıf
        /// </summary>
        private static ServerInit serverInit;

        /// <summary>
        /// Card Connector yardımcı nesne
        /// </summary>
        private static CrdConnectorHelper crdConnectorHelper;

        #endregion

        #region Properties

        public static ServerInit GetInstance()
        {
            if (serverInit == null)
                serverInit = new ServerInit();
            return serverInit;
        }

        public static CrdConnectorHelper CrdConnectorHelper
        {
            get
            {
                if (crdConnectorHelper == null)
                {
                    switch (ConfigurationManager.AppSettings.Get("PosType"))
                    {
                        case "OLIVETTI":
                            crdConnectorHelper = new CrdConnectorHelper(new OlivettiConnector());
                            break;
                        case "INGENICO":
                            crdConnectorHelper = new CrdConnectorHelper(new IngenicoConnector());
                            break;
                        case "HUGIN":
                            crdConnectorHelper = new CrdConnectorHelper(new HuginConnector());
                            break;
                        default:
                            throw new InstanceNotFoundException(string.Format("{0} pos tipi bulunamadı.",
                                ConfigurationManager.AppSettings.Get("PosType")));
                    }

                };

                return crdConnectorHelper;
            }
        }
        #endregion

    }
}
