using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class AddQuotaQuantityCommand : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
    }
}