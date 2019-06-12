

using System.Collections.Generic;

namespace DynamicPos.CrdData.Model
{
    /// <summary>
    /// Satışa ait kalemlerin tutulduğu sınıf.
    /// </summary>
    public class SaleItemDataModel
    {

        #region Properties

        /// <summary>
        /// Departman indeksi
        /// </summary>
        public string DepartmentIndex { get; set; }

        public List<TransItemModel> TransItems{ get; set; }

        #endregion 

    }

    /// <summary>
    /// Fişe yazdırılacak ürün bilgilerinin tutulduğu model bilgisi.
    /// </summary>
    public class TransItemModel
    {

        #region Properties

        /// <summary>
        /// Satış miktarı (Length: 4)
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Tüm koşullar(indirim,arttırım gibi) uygulandıktan sonra
        /// satış kaleminin son fiyatı. (kuruş)
        /// </summary>
        public string FinalAmount { get; set; }

        /// <summary>
        /// Material adı
        /// </summary> 
        public string MaterialName { get; set; }

        /// <summary>
        /// Birim Fiyat.
        /// Gönderildiği taktirde tutar dikkate alınmaz. (Length : 6)
        /// </summary>
        public string UnitPrice { get; set; }

        #endregion

    }
}
