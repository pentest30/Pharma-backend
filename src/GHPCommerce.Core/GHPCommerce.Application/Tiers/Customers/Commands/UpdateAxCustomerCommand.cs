using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class UpdateAxCustomerCommand : ICommand<ValidationResult>
    {
        public string Code { get; set; }
        public string DefaultSalesPerson { get; set; }
        public string DefaultSalesGroup { get; set; }
        public string DefaultDeliverySector { get; set; }
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }
        public int PaymentDeadline { get; set; }
        public CustomerState CustomerState { get; set; }
        public string CustomerGroup { get; set; }
        public decimal MonthlyObjective { get; set; }
        public string PaymentMethod { get; set; }
    }
}
