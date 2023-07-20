using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class PrintPreparationOrderCommand : ICommand<ValidationResult>
    {
        public List<Guid> OrdersIds { get; set; }
    }
}
