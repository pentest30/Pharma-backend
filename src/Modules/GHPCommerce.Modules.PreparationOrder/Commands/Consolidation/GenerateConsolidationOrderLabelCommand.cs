using GHPCommerce.Domain.Domain.Commands;
using System;
using FluentValidation.Results;

namespace GHPCommerce.Modules.PreparationOrder.Commands.Consolidation
{
    public class GenerateConsolidationOrderLabelCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
}
