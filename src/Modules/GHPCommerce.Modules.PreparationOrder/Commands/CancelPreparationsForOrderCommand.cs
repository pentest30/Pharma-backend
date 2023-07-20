using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class CancelPreparationsForOrderCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }
}