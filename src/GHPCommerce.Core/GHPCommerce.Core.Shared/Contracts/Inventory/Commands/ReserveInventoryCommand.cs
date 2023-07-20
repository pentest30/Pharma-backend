using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class ReserveInventoryCommand : ICommand<ValidationResult>
    {
        public Dictionary<Guid,double> Reservations { get; set; }
    }
}