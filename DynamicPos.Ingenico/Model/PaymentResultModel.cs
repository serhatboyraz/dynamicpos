using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    public class PaymentResultModel
    {
        /// <summary>
        /// Para üstü
        /// </summary>
        public double CashBackAmount { get; set; }

        /// <summary>
        /// Kalan tutar
        /// </summary>
        public double RemainingAmount { get; set; }

        /// <summary>
        /// Ödenen Tutar
        /// </summary>
        public double PayAmount { get; set; }

        public string MerchantId { get; set; }

        public byte NumberOfBonus { get; set; }
        public byte  NumberOfDiscount { get; set; }

        public string TerminalId { get; set; }

        /// <summary>
        /// Ödenen ve kalan bilgisini tutar.
        /// </summary>
        public string Display { get; set; }

        public uint Stan { get; set; }

        public uint BatchNo { get; set; }

        public ushort BankBkmId { get; set; }

        public string AuthorizeCode { get; set; }

        public string BankName { get; set; }

        public string CardHolderName { get; set; }

        public string CardExpireDate { get; set; }

        public string CardPan { get; set; }
    }
}
