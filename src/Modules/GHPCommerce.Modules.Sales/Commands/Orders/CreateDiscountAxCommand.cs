using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class CreateDiscountAxCommand: ICommand<ValidationResult>
    {
        public int ThresholdQuantity { get; set; }
        public decimal DiscountRate { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string ProductCode { get; set; }
    }
    public class CreateDiscountAxCommandValidator : AbstractValidator<CreateDiscountAxCommand>
    {
        public CreateDiscountAxCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ThresholdQuantity)
                .GreaterThan(0);
            RuleFor(v => v.From)
                .Must(x =>x!= default);
            RuleFor(v => v.To)
                .Must(x => x != default);
        }
    }
}
