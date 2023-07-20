using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class UpdateOrderStatusCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public int OrderStatus { get; set; }

    }
}
