using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmaVendor.CRD.Model.DataModel;

namespace UmaVendor.CRD.Model
{
    /// <summary>
    /// Satışa ait ortak alanların bulunduğu sınıf.
    /// </summary>
    public class SaleHeaderDataModel
    {
        /// <summary>
        /// Auto Increment ID
        /// </summary> 
        public int Id { get; set; }

        /// <summary>
        /// FIXME: Pos Sistemine atanan id
        /// </summary> 
        public string PosId { get; set; }

        /// <summary>
        /// Ökc Terminal Seri numarası
        /// </summary> 
        public string CrdId { get; set; }

        /// <summary>
        /// Pos sisteminde işlemin gerçekleştiği tarih-saat
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Harici referans numarası (mal giriş/ çıkış fiş numarası)
        /// TODO: TerminalSeriNo/ZNo/FişNo gibi bir kombinasyon kullanılacak ise alan unique olmalı
        /// </summary> 
        public string DocNum { get; set; }


        /// <summary>
        /// Müşteri numarası
        /// </summary> 
        public string CustomerNum { get; set; }

        /// <summary>
        /// Müşteri Adı
        /// </summary> 
        public string CustomerName { get; set; }

        /// <summary>
        /// Kasiyer Kullanıcı Adı veya Kodu
        /// </summary>  
        public string CashierUserName { get; set; }

        /// <summary>
        /// Kasiyer Adı
        /// </summary> 
        public string CashierName { get; set; }

        /// <summary>
        /// Kasiyer Soyadı
        /// </summary> 
        public string CashierSurname { get; set; }

        /// <summary>
        /// Satış Tipi Kodu
        /// </summary>
        public int SaleTypeCode { get; set; }

        /// <summary>
        /// Satış Tipi Adı
        /// </summary> 
        public string SaleTypeName { get; set; }

        /// <summary>
        /// Satılan ürün listesi
        /// </summary>
        public virtual ICollection<SaleItemDataModel> SaleItems { get; set; }

        /// <summary>
        /// Satışa ait ödeme listesi
        /// </summary>
        public virtual ICollection<SalePaymentDataModel> SalePayments { get; set;} 


    }
}
