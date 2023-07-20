using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class CreateQuotaCommand : ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public int InitialQuantity { get; set; }
        public DateTime QuotaDate { get; set; }
        public bool IsDemand { get; set; }
        public Guid SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
    }
    

    public class CreateQuotaCommandValidator : AbstractValidator<CreateQuotaCommand>
    {
        public CreateQuotaCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductName)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.CustomerName)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.CustomerCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);
            
        }
    }
}