namespace GHPCommerce.Application.Tiers.Customers.DTOs
{
    public class DashboardDto
    {
         
        public decimal CA { get; set; }
        public decimal MonthlyObjective { get; set; }
        public decimal TotalOrdersMonthly { get; set; }
        public decimal OrderTotalMonthBenefit { get; set; }
        public decimal OrderTotalMonthBenefitRate { get; set; }
        public int OrdersPerMonth { get; set; }
        public decimal OrderTotalMonthPurchasePrice { get; set; }
        public decimal DailyMarkUpRate { get; set; }
        public decimal MonthlyMarkUpRate => TotalOrdersMonthly == 0 ? 0 : OrderTotalMonthBenefit / TotalOrdersMonthly;

    }
}
