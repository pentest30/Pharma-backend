using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Application.Catalog.Manufacturers.Commands
{
    public class UpdateManufacturerCommand :ICommand<ValidationResult>
    {
        public UpdateManufacturerCommand()
        {
            Id = Guid.NewGuid();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<EmailModel> Emails { get; set; }
        public Guid Id { get; set; }
    }
    public class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturerCommand>
    {
        public UpdateManufacturerCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
            RuleForEach(x => x.Addresses)
                .SetValidator(new AddressValidator());
            RuleForEach(x => x.PhoneNumbers)
                .SetValidator(new PhoneNumberValidator());
        }
    }
}
