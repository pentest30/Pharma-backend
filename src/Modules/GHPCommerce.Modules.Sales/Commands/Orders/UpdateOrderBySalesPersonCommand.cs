using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class UpdateOrderBySalesPersonCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }

        public string SupplierName { get; set; }
        public uint OrderStatus { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
