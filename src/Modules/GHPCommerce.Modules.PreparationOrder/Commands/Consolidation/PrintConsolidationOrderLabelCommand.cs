using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.PreparationOrder.Commands.Consolidation
{
    public class PrintConsolidationOrderLabelCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
}
