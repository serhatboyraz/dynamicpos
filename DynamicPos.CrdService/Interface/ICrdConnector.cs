using DynamicPos.CrdData.Model;

namespace DynamicPos.CrdService.Interface
{
    /// <summary>
    /// 2 pos cihazı için interface'imiz 
    /// </summary>
    public interface ICrdConnector
    {
        /// <summary>
        /// Bağlan ve işlem yap.
        /// Bağlandıysa tekrar bağlanmayı deneme..
        /// </summary>
        /// <param name="allProcessModel"></param>
        /// <returns></returns>
        DoAllProcessResultModel DoAllProcess(DoAllProcessModel allProcessModel);

        /// <summary>
        /// Pos Bağlantı durumunu getirir.
        /// </summary>
        /// <returns></returns>
        ConnectionResultModel IsConnected();

        /// <summary>
        /// Pos HandShake işlemini gerçekleştirir.
        /// </summary>
        /// <param name="handShakeModel">Port KasiyerInformation, Ecr Mode...</param>
        /// <returns>HandShakeResultModel döndürür.</returns>
        HandShakeResultModel HandShake(HandShakeModel handShakeModel);

        /// <summary>
        /// Fiş Başlık yazdırma işlemini gerçekleştirir.
        /// </summary>
        /// <param name="headerModel"></param>
        /// <returns></returns>
        DocumentHeaderResultModel ReceiptBegin(DocumentHeaderModel headerModel);

        /// <summary>
        /// Do Transaction İşlemini gerçekleştirir.
        /// </summary>
        /// <param name="saleItemModel"></param>
        /// <returns></returns>
        SaleItemResultModel ItemSale(SaleItemDataModel saleItemModel);

        /// <summary>
        /// Do PAyment İşlemini gerçekleştirir.
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        GetPaymentResultModel GetPayment(SalePaymentDataModel payment);

        /// <summary>
        /// Ödemeyi iptal et.
        /// </summary>
        /// <param name="cancelPaymentModel"></param>
        /// <returns></returns>
        CancelPaymentResultModel CancelPayment(CancelPaymentModel cancelPaymentModel);

        /// <summary>
        /// Fişi Sonlandırma işlemini gerçekleştirir.
        /// </summary>
        /// <returns></returns>
        EndReceiptResultModel EndReceipt();

        /// <summary>
        /// Fişi iptal eder.
        /// </summary>
        /// <returns></returns>
        CancelDocumentResultModel CancelDocument();

        /// <summary>
        /// Toplam Ücreti getirir.
        /// </summary>
        /// <returns></returns>
        SumAmountResultModel GetSumAmount();

        /// <summary>
        /// Toplam Ücreti getirir.
        /// </summary>
        /// <returns></returns>
        void ChangeMode(string mode);


        /// <summary>
        /// Günlük x raporu al
        /// </summary>
        void GetDailyXReport();

        /// <summary>
        /// Günlük z raporu al
        /// </summary>
        void GetDailyZReport();

    }
}
