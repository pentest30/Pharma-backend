using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class ChangeOrderStateCommand :ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }

        public OrderStatus orderStatus { get; set; }
        public CancellationReason CancellationReason { get; set; }
        public string RejectedReason { get; set; }

    }
}
