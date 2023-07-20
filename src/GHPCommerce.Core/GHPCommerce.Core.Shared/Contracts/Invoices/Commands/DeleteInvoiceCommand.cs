using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Invoices.Commands
{
    public class DeleteInvoiceCommand :  ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }
}