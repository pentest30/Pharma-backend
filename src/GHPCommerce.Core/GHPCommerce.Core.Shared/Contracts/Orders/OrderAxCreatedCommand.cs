using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders
{
    public class OrderAxCreatedCommand : ICommand<ValidationResult>
    {
        public int OrderNumber { get; set; }
        public string CodeAx { get; set; }
    }
}