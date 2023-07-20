using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class OrderItemDeleteCommand : ICommand<ValidationResult>
    {
        //public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SupplierId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        
        
    }
}