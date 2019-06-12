using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    /// <summary>
    /// Satışa ait ödemelerin tutulduğu sınıf.
    /// </summary>
    public class SalePaymentDataModel
    {
        /// <summary>
        /// Auto Increment Id
        /// </summary> 
        public int Id { get; set; }

        /// <summary>
        /// Ödeme Türü Kodu
        /// </summary>
        public int PaymentTypeCode { get; set; }

        /// <summary>
        /// Ödeme Türü Adı
        /// </summary> 
        public string PaymentTypeName { get; set; }

        /// <summary>
        /// Kur çevrimi default ödeme türüne göre yapıldıktan sonraki ödeme toplamı
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// ISO 4217 Para birimi kodu
        /// https://tr.wikipedia.org/wiki/ISO_4217
        /// </summary> 
        public string ExchangeCode { get; set; }

        /// <summary>
        /// ISO 4217 Para birimi no
        /// https://tr.wikipedia.org/wiki/ISO_4217
        /// </summary>
        public ushort ExchangeShortCode { get; set; }


        /// <summary>
        /// Uygulanan Kur Çevrim Oranı
        /// </summary>
        public double ExchangeRate { get; set; }

        /// <summary>
        /// Hesap Sahibi - Kart üzerindeki isim olabilir
        /// </summary> 
        public string AccountOwner { get; set; }


        /// <summary>
        /// Banka Numarası - BkmId olabilir
        /// </summary> 
        public ushort BkmId{ get; set; }

        /// <summary>
        /// Çevrimiçi provizyon için provizyon numarası
        /// </summary> 
        public string ProvisionNum { get; set; }

        /// <summary>
        /// Ödeme işlemi Tarih-Saati
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Taksit Sayısı
        /// </summary>
        public ushort InstallmentCount { get; set; }

        /// <summary>
        /// Ödemenin yapıldığı ana satış nesnesi
        /// </summary>
        public virtual SaleHeaderDataModel SaleHeader { get; set; }



    }
}
