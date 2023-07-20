using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class UpdateOrderDiscountCommandV2 : ICommand<ValidationResult>
    {
        public UpdateOrderDiscountCommandV2()
        {
           DiscountLines = new List<DiscountLine>();
        }
        public Guid CustomerId { get; set; }
        public Guid Id { get; set; }
        public List<DiscountLine> DiscountLines { get; set; }
        
    }

    public class DiscountLine
    {
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
        public double Discount { get; set; }
    }
}