using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class DecreaseQuotaCommand : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid CustomerId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public Guid SalesPersonId { get; set; }


    }
    public class IncreaseQuotaCommandV2 : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SalesPersonId { get; set; }
        public Guid OrganizationId { get; set; }
    }
}