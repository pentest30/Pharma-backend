using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class UpdateAxCustomerDebtCommand :ICommand<ValidationResult>
    {
        public string Code { get; set; }
        public decimal Debt { get; set; }
    }
}