using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model.DataModel
{
    /// <summary>
    /// Satışa ait kalemlerin tutulduğu sınıf. Fiyatlar SaleItemCondition'da..
    /// </summary>
    public class SaleItemDataModel
    {
        /// <summary>
        /// Auto Increment Id
        /// </summary> 
        public int Id { get; set; }

        /// <summary>
        /// Fiş içerisindeki index
        /// </summary>
        public ushort TicketIndex { get; set; }

        /// <summary>
        /// Ölçü birimine bağlı miktar. 1,25 kilo gibi.
        /// </summary>
        public double? UnitQuantity { get; set; }


        /// <summary>
        /// Adet
        /// </summary>
        public uint Quantity { get; set; }

        /// <summary>
        /// Tüm koşullar(indirim,arttırım gibi) uygulandıktan sonra
        /// satış kaleminin son fiyatı.
        /// </summary>
        public double FinalAmount { get; set; }

        /// <summary>
        /// Koşullar uygulanmadan önceki fiyatı
        /// </summary>
        //public double FirstAmount { get; set; }


        /// <summary>
        /// Malzeme numarası
        /// </summary>
        public uint TaxValue { get; set; }

        /// <summary>
        /// Departman indeksi
        /// </summary>
        public byte DepartmentIndex { get; set; }
        /// <summary>
        /// Malzeme numarası
        /// </summary> 
        public string MaterialCode { get; set; }

        /// <summary>
        /// Material adı
        /// </summary> 
        public string MaterialName { get; set; }

        /// <summary>
        /// Okutulan Barkod Numarası
        /// </summary> 
        public string EAN11 { get; set; }


        /// <summary>
        /// Satıcı Kullanıcı Adı veya Kodu
        /// </summary> 
        public string SellerUserName { get; set; }

        /// <summary>
        /// Satıcı Adı
        /// </summary> 
        public string SellerName { get; set; }

        /// <summary>
        /// Satıcı Soyadı
        /// </summary>  
        public string SellerSurname { get; set; }
       
    }
}
