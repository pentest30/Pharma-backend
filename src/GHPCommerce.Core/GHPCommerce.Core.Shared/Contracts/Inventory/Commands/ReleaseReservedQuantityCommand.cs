using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class ReleaseReservedQuantityCommand : ICommand<ValidationResult>
    {
        public Dictionary<Guid, int> ReleasedQuantities   { get; set; }
    }
}