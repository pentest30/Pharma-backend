using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    [Serializable]
    public class CreateOrderDraftByPharmacistCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid? CurrentUserId { get; set; }
        public Guid SupplierId { get; set; }
        public List<OrderItemDTo> OrderItemDTos { get; set; }

    }
    [Serializable]
    public class OrderItemDTo
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid InventSumId { get; set; }
    }
}
