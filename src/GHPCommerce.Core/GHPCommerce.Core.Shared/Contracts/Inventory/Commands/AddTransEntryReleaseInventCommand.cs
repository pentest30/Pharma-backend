using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class AddTransEntryReleaseInventCommand : ICommand<ValidationResult>
    {
        public int Status { get; set; }
        public Guid InventId { get; set; }
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }

    }
}
