using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Commands
{
    public class FeedInventoryCommand: ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    
        public double PhysicalOnhandQuantity { get; set; }
    

    }
}