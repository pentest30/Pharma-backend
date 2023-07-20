using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.ShoppingCarts.Commands
{
    public class CreateShoppingCartItemCommand :ICommand<ValidationResult>
    {
        public CreateShoppingCartItemCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
    }
}
