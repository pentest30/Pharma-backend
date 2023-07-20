namespace GHPCommerce.Core.Shared.Contracts.Invoices.DTOs
{
    public class InvoiceDtoV3
    { 

        public decimal OrderTotal { get; set; }
        public decimal OrderTotalMonth { get; set; }
        public decimal OrderTotalMonthBenefit { get; set; }
        public decimal OrderTotalMonthBenefitRate { get; set; }
        public decimal OrderTotalMonthPurchasePrice { get; set; }

        public int OrdersPerMonth { get; set; }
        public decimal MonthlyMarkUpRate => OrderTotalMonth == 0 ? 0 : OrderTotalMonthBenefit / OrderTotalMonth;
        public decimal DailyMarkUpRate { get; set; }
    }
}
