using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    public enum PaymentTypeEnum
    {
        PAYMENT_ALL = 0x000FFFFF,	        //  NAKIT  KREDI  OTHER  YCEKI  DOVIZ   MATRAH  MENU(ODEME TIPLERI)
        PAYMENT_CASH_TL = 0x00000001,	    // 	++++   xxxx   xxxx   xxxx   xxxx    ++++	xxxx
        PAYMENT_CASH_CURRENCY = 0x00000002,	// 	xxxx   xxxx   xxxx   xxxx   ++++    ++++    xxxx
        PAYMENT_BANK_CARD = 0x00000004,	    //	xxxx   ++++   xxxx   xxxx   xxxx    ++++    xxxx
        PAYMENT_YEMEKCEKI = 0x00000008,	    //	xxxx   xxxx   xxxx   ++++   xxxx    xxxx    xxxx(Uygulama varsa)
        PAYMENT_MOBILE = 0x00000010,	    // 	xxxx   ++++   xxxx   xxxx   xxxx    ++++    xxxx(Uygulama varsa)
        PAYMENT_HEDIYE_CEKI = 0x00000020,   // 	xxxx   xxxx   ++++   xxxx   xxxx    ++++    ++++
        PAYMENT_IKRAM = 0x00000040,         // 	xxxx   xxxx   ++++   xxxx   xxxx    ++++    ++++
        PAYMENT_ODEMESIZ = 0x00000080,      // 	xxxx   xxxx   ++++   xxxx   xxxx    ++++    ++++
        PAYMENT_KAPORA = 0x00000100,        // 	xxxx   xxxx   ++++   xxxx   xxxx    ++++    ++++
        PAYMENT_PUAN = 0x00000200,          // 	xxxx   xxxx   ++++   xxxx   xxxx    ++++    ++++

        //REVERSE_PAYMENT_ALL = 0xFFF00000,     //açılacak
        REVERSE_PAYMENT_CASH = 0x00100000,
        REVERSE_PAYMENT_BANK_CARD_VOID = 0x00200000,
        REVERSE_PAYMENT_BANK_CARD_REFUND = 0x00400000,
        REVERSE_PAYMENT_YEMEKCEKI = 0x00800000,
        REVERSE_PAYMENT_MOBILE = 0x01000000,
        REVERSE_PAYMENT_HEDIYE_CEKI = 0x02000000,
    };
}
