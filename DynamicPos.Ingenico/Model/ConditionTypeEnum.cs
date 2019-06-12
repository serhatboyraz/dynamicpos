using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaVendor.CRD.Model
{
    public enum ConditionTypeEnum
    {
        ITEM_AMOUNT_DECREASE = 1,
        ITEM_AMOUNT_INCREASE = 2,
        ITEM_PERCENT_DECREASE = 3,
        ITEM_PERCENT_INCREASE = 4,
        RECEIPT_AMOUNT_DECREASE = 5,
        RECEIPT_AMOUNT_INCREASE = 6,
        RECEIPT_PERCENT_DECREASE = 7,
        RECEIPT_PERCENT_INCREASE = 8
    }
}
