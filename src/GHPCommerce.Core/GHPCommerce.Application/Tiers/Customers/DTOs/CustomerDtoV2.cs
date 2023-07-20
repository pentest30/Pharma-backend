using GHPCommerce.Domain.Domain.Tiers;
using System;

namespace GHPCommerce.Application.Tiers.Customers.DTOs
{
    public class CustomerDtoV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OrganizationId { get; set; }
        public string SalesGroup { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public string DefaultSalesPersonName { get; set; }
        public string CustomerStatus { get; set; }
        public CustomerState CustomerState { get; set; }
        public bool HasOrderToDay { get; set; }
        public decimal CA { get; set; }
        public decimal MonthlyObjective { get; set; }
        public decimal TotalOrdersMonthly { get; set; }
        public decimal OrderTotalMonthBenefit { get; set; }
        public decimal OrderTotalMonthBenefitRate { get; set; }
        public int OrdersPerMonth { get; set; }
        public decimal OrderTotalMonthPurchasePrice { get; set; }
        public decimal MonthlyMarkUpRate => TotalOrdersMonthly==0?0: OrderTotalMonthBenefit / TotalOrdersMonthly;
        public decimal DailyMarkUpRate { get; set; }



    }
}
