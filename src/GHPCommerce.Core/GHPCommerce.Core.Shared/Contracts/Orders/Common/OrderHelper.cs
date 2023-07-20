using System;
using System.Collections.Generic;
using System.Linq;
using GHPCommerce.Core.Shared.Contracts.Cache;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Common
{
    public static class OrderHelper
    {

        public static decimal CalculateTotalIncludeDiscount(this List<CachedOrderItem> orderOrderItems)
        {
            return Math.Round(orderOrderItems.Sum(i => i.Quantity * i.UnitPrice * (1 - ((decimal)i.Discount + i.ExtraDiscount))), 2);
        }
        public static decimal CalculateTotalExlDiscountTax(this List<CachedOrderItem> orderOrderItems)
        {
            return Math.Round(orderOrderItems.Sum(i => i.Quantity * i.UnitPrice));
        }

        public static decimal CalculateTotalIncTax(this List<CachedOrderItem> orderOrderItems)
        {
            return Math.Round(orderOrderItems.Sum(i => i.Quantity * i.UnitPrice * (1 - ((decimal)i.Discount + i.ExtraDiscount)) * (1 + (decimal)i.Tax)), 2);
        }
        public static decimal CalculateOrderBenefit(this List<CachedOrderItem> orderOrderItems)
        {
            return orderOrderItems.Sum(i =>
               i.Quantity * i.UnitPrice * (1 - ((decimal)i.Discount + i.ExtraDiscount))
                - i.Quantity * i.PurchaseUnitPrice
                );
        }
        public static decimal CalculateGlobalRateBenefit(this List<CachedOrderItem> orderOrderItems, decimal globalBenefit)
        {
            var sumPurchasePrice = orderOrderItems.Sum(i =>i.Quantity* i.PurchaseUnitPrice);
            return sumPurchasePrice==0?0: globalBenefit / sumPurchasePrice;
        }
        public static decimal CalculateGlobalRateMarque(List<CachedOrderItem> orderOrderItems, decimal globalBenefit)
        {

            var sumSalesPrice = orderOrderItems.Sum(i =>i.Quantity * i.UnitPrice* (1- (decimal)i.Discount- (decimal)i.ExtraDiscount));
            if (sumSalesPrice == 0) return 0;
            return Math.Round(globalBenefit / sumSalesPrice);
        }
        public static decimal CalculateDiscount(this List<CachedOrderItem> orderOrderItems)
        {
            return orderOrderItems.Sum(i =>i.Quantity*i.UnitPrice*(decimal) i.Discount);
        }
           
        
    }
}
