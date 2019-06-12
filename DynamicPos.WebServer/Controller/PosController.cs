using System;
using System.Net;
using DynamicPos.CrdData.Model;
using DynamicPos.Utils.Helpers;
using DynamicPos.WebServer.Helper;
using Newtonsoft.Json;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

namespace DynamicPos.WebServer.Controller
{

    /// <summary>
    /// Pos Controller Sınıfı
    /// </summary>
    public class PosController : WebApiController
    {

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">HttpContext</param>
        public PosController(IHttpContext context)
            : base(context)
        {
        }

        ~PosController()
        {
            Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pos Bağlantı Durumunu getirir.
        /// </summary>
        /// <returns>Bağlantı varsa true, yoksa false</returns>
        [WebApiHandler(HttpVerbs.Get, "/Pos/CheckConnection")]
        public bool CheckConnection()
        {
            try
            {
                ConnectionResultModel connectionResult = ServerHelper.CrdConnectorHelper.CrdConnector.IsConnected();
                if (connectionResult.StatusCode == HttpStatusCode.OK)
                    Response.SetRespose(HttpStatusCode.OK, connectionResult);
                else
                    Response.SetRespose(HttpStatusCode.BadRequest, connectionResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Fiş başlığı yazdırır.
        /// </summary>
        /// <returns>ctx exception için bool true döner.</returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/StartReceipt")]
        public bool StartReceipt()
        {
            try
            {
                DocumentHeaderModel documentHeader = GetContextModel<DocumentHeaderModel>();
                DocumentHeaderResultModel response = ServerHelper.CrdConnectorHelper.CrdConnector.ReceiptBegin(documentHeader);

                Response.SetRespose(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                Logger.Error("Pos_StartReceipt_Error\n" + ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Ürüleri fişe yazdırır.
        /// Do Transaction
        /// </summary>
        /// <returns>Satılan ürünlerin listesi Json olarak döndürülür</returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/PrintItem")]
        public bool PrintItem()
        {
            try
            {
                SaleItemDataModel saleItemModel = GetContextModel<SaleItemDataModel>();

                if (saleItemModel != null)
                {
                    SaleItemResultModel saleItemResult = ServerHelper.CrdConnectorHelper.CrdConnector.ItemSale(saleItemModel);
                    Response.SetRespose(HttpStatusCode.OK, saleItemResult);
                }
                else
                    Response.SetRespose(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Ödeme yapar.
        /// </summary>
        /// <returns>Ödemesi yapılacak ürün bilgisini döndürür.</returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/PrintPayment")]
        public bool PrintPayment()
        {
            try
            {

                SalePaymentDataModel salePaymentDataModel = GetContextModel<SalePaymentDataModel>();
                if (salePaymentDataModel != null)
                {

                    GetPaymentResultModel response = ServerHelper.CrdConnectorHelper.CrdConnector.GetPayment(salePaymentDataModel);

                    Logger.Info(string.Format("PrintPayment()", response));
                    Response.SetRespose(response.HttpStatusCode, response);
                }
                else
                {
                    Response.SetRespose(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Ödemeyi iptal et
        /// </summary>
        /// <returns></returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/CancelPayment")]
        public bool CancelPayment()
        {
            try
            {
                CancelPaymentModel cancelPaymentModel = GetContextModel<CancelPaymentModel>();
                if (cancelPaymentModel != null)
                {
                    CancelPaymentResultModel response = ServerHelper.CrdConnectorHelper.CrdConnector.CancelPayment(cancelPaymentModel);
                    Logger.Info(string.Format("PrintPayment()", response));
                    Response.SetRespose(response.HttpStatusCode, response);
                }
                else
                {
                    Response.SetRespose(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Fişi yazdırır.     
        /// </summary>
        /// <returns>Sonlandırma başarılıysa true, değilse false</returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/CloseReceipt")]
        public bool CloseReceipt()
        {
            try
            {
                EndReceiptResultModel response = ServerHelper.CrdConnectorHelper.CrdConnector.EndReceipt();
                Logger.Info(string.Format("CloseDocument()\n {0} ", response));
                Response.SetRespose(response.HttpStatusCode, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Fişi iptal eder
        /// </summary>
        /// <returns></returns>
        [WebApiHandler(HttpVerbs.Post, "/Pos/CancelDocument")]
        public bool CancelDocument()
        {
            try
            {
                CancelDocumentResultModel response = ServerHelper.CrdConnectorHelper.CrdConnector.CancelDocument();
                Logger.Info(string.Format("CancelDocument()\n {0} ", response));
                Response.SetRespose(response.HttpStatusCode, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Toplam Tutarı Getirir.
        /// </summary>
        /// <returns></returns>
        [WebApiHandler(HttpVerbs.Get, "/Pos/GetSumTotal")]
        public bool GetSumTotal()
        {
            try
            {
                SumAmountResultModel sumAmountResult = ServerHelper.CrdConnectorHelper.CrdConnector.GetSumAmount();
                Logger.Info(string.Format("CancelDocument()\n {0} ", sumAmountResult));
                Response.SetRespose(sumAmountResult.HttpStatusCode, sumAmountResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        [WebApiHandler(HttpVerbs.Post, "/Pos/DoAllProcess")]
        public bool ConnectionAndDoProcess()
        {
            DoAllProcessModel allProcessModel;
            try
            {
                allProcessModel = GetContextModel<DoAllProcessModel>();
                DoAllProcessResultModel allProcessResultModel =
                    ServerHelper.CrdConnectorHelper.CrdConnector.DoAllProcess(allProcessModel);

                Response.SetRespose(HttpStatusCode.OK, allProcessResultModel);
            }
            catch (Exception ex)
            {
                Logger.Error("Pos_Connection_Error\n" + ex.Message, ex);
                Response.SetRespose(HttpStatusCode.InternalServerError);
            }

            return true;
        }

        /// <summary>
        /// Formatta, gelen requestin bodymodel olarak dönüşünü sağlar
        /// </summary>
        /// <typeparam name="T">Dönecek model, tür</typeparam>
        /// <returns>RequestBody Model, tür</returns>
        private T GetContextModel<T>()
        {
            string requestBody = this.RequestBody();
            Logger.Info(requestBody);
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        /// <summary>
        /// Dispose işlemi
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        #endregion

    }
}
