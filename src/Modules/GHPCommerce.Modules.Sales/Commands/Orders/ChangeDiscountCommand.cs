using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    [Obsolete("this class is Obsolete, please use UpdateOrderDiscountCommandV2 class instead of this class")]
    public class ChangeDiscountCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public double Discount { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid ProductId { get; set; }
        public Guid CustomerId { get; set; }

    }
}
