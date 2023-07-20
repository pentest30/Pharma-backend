using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class UpdatePhysicalReservedQuantityCommand : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
        public int Quantity { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
