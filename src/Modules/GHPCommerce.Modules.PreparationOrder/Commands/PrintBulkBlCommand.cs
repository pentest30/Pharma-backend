using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class PrintBulkBlCommand : ICommand<ValidationResult>
    {
        public List<Guid> Ids { get; set; }
    }
}
