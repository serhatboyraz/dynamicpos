using System;
using DynamicPos.CrdService.Interface;

namespace DynamicPos.CrdService.Helper
{
    /// <summary>
    /// Tüm pos cihazı işlemleri bu sınıf üzerinden gerçekleştirelecektir.
    /// </summary>
    public class CrdConnectorHelper : IDisposable
    {

        #region Properties

        public ICrdConnector CrdConnector { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crdConnector"></param>
        public CrdConnectorHelper(ICrdConnector crdConnector)
        {
            CrdConnector = crdConnector;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        #endregion

    }
}
