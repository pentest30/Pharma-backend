using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Dtos
{
    public class OrderDtoV1
    {
        public Guid CustomerId { get; set; }

        public decimal OrderTotal { get; set; }
        public decimal OrderTotalMonth { get; set; }
        public decimal OrderTotalMonthBenefit { get; set; }
        public decimal OrderTotalMonthBenefitRate { get; set; }
        public decimal OrderTotalMonthPurchasePrice { get; set; }

        public int OrdersPerMonth { get; set; }
        public decimal MonthlyMarkUpRate { get; set; }
        public decimal DailyMarkUpRate { get; set; }
        
    }
}
