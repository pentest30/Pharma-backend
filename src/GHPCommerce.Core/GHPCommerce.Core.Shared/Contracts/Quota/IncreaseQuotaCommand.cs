using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class IncreaseQuotaCommand : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SalesPersonId { get; set; }

    }
}