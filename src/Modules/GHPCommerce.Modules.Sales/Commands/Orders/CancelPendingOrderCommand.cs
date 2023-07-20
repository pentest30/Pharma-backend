using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class CancelPendingOrderCommand : ICommand<ValidationResult>
    {
        public OrderDto Order { get; set; }
       
    }
}
