using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.Entities;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class UpdatePreparationOrderItem: ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid PreparationOrderItemId { get; set; }
        public int Quantity { get; set; }
        public int? OldQuantity { get; set; }
        public string InternalBatchNumber { get; set; }
        
        public string VendorBatchNumber { get; set; }
        public BlStatus Status { get; set; }

    }
}