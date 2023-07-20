using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class CreateQuotaRequestCommand : ICommand<ValidationResult>
    {
        public CreateQuotaRequestCommand()
        {
            Date = DateTime.Now;
        }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public bool ForSuperVisor { get; set; }
        public bool ForBuyer { get; set; }
        public Guid? DestSalesPersonId { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class CreateQuotaRequestCommandValidator : AbstractValidator<CreateQuotaRequestCommand>
    {
        public CreateQuotaRequestCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).NotEmpty();
            RuleFor(v => v.ProductName)
                .MaximumLength(200).NotEmpty();

            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);

        }
    }
}