using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    public class BankPaymentAppInfo
    {
        public string Name { get; set; }

        public ushort BkmId { get; set; }
        
        public ushort Priority { get; set; }

        public ushort Status { get; set; }

        public ushort InstalmentCount { get; set; }


    }
}
