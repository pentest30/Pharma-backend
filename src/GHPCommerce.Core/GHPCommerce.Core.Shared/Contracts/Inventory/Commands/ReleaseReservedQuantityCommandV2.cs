using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class ReleaseReservedQuantityCommandV2 :ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
        public double Quantity { get; set; }
    }
    public class ReleaseReservedQuantityCommandV3 : ICommand<ValidationResult>
    {
        public List<ReleaseReservedQuantityCommandV2> QuantitiesToRelease { get; set; }
        public Guid OrganizationId { get; set; }
    }
}