using Newtonsoft.Json;

namespace DynamicPos.CrdData.Model
{
    public class DocumentHeaderModel
    {

        #region Member

        /// <summary>
        /// Döküman tipi
        /// </summary>
        [JsonProperty("DocumentType")]
        public string DocType { get; set; }

        /// <summary>
        /// Müşteri Tc numarası
        /// </summary>
        [JsonProperty("TCKN")]
        public string CustomerTcNo { get; set; }

        [JsonProperty("VKN")]
        public string CustomerVkNo { get; set; }

        #endregion

    }

}
