using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class ChangePaymentStateCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
