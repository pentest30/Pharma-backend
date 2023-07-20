using System;

namespace GHPCommerce.Core.Shared.Contracts.Invoices.DTOs
{
    public class InvoiceDtoV2
    {
        public Guid CustomerId { get; set; }

        public decimal OrderTotal { get; set; }
        public decimal orderTotalMonth { get; set; }
        public decimal orderTotalMonthBenefit { get; set; }
        public decimal orderTotalMonthBenefitRate { get; set; }
        public decimal orderTotalMonthPurchasePrice { get; set; }

        public int ordersPerMonth { get; set; }
        public decimal MonthlyMarkUpRate => orderTotalMonth == 0 ? 0 : orderTotalMonthBenefit / orderTotalMonth;
        public decimal DailyMarkUpRate { get; set; }
    }
}
