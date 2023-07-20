using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.SalesInvoice
{
    public class CreateInvoiceCommand : ICommand<ValidationResult>
    {
        public Guid DeliveryOrderId { get; set; }

    }
}