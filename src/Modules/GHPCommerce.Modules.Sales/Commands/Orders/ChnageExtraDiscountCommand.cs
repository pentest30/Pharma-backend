using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class ChangeExtraDiscountCommand :ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public decimal Discount { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid ProductId { get; set; }
        public Guid CustomerId { get; set; }
       
    }
}
