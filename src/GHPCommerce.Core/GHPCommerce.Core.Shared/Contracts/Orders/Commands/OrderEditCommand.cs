using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class OrderEditCommand : ICommand<EditOrderContract>
    {
        public EditOrderContract Order { get; set; }
    }
}