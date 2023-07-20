using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Invoices
{
    public class ValidateInvoiceCommand :  ICommand<ValidationResult>
    {
        public Guid InvoiceId { get; set; }
    }
}