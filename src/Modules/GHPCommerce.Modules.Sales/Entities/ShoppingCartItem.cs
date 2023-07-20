using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Sales.Entities
{
    public class ShoppingCartItem : AggregateRoot<Guid>
    {
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal Total { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalTax { get; set; }
        public Guid? GuestId { get; set; }

    }
}
