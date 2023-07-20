using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class CreateDiscountCommand : ICommand<ValidationResult>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public int ThresholdQuantity { get; set; }
        public decimal DiscountRate { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string ProductFullName { get; set; }


    }
    public class CreateDiscountCommandValidator : AbstractValidator<CreateDiscountCommand>
    {
        public CreateDiscountCommandValidator()
        {
          
        }
    }
}
