using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    public class TicketInfoModel
    {
        /***
 * 
 * ushort ejno
 * ushort receiptno
 * ushort zno
 * datetime ticketdate
 * */
        public ushort EJNo { get; set; }

        public ushort ZNo { get; set; }

        public ushort ReceiptNo { get; set; }

        public DateTime TicketDate { get; set; }
    }
}
