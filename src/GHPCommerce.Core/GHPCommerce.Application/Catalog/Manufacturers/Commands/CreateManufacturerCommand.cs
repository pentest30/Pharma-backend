using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Application.Catalog.Manufacturers.Commands
{
    public class CreateManufacturerCommand :ICommand<ValidationResult>
    {
        public CreateManufacturerCommand()
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

    public class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
    {
        public CreateManufacturerCommandValidator()
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

    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(v => v.Country)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.State)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Township)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.City)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Street)
                .MaximumLength(500).
                NotEmpty();
          
        }
    }

    public class PhoneNumberValidator : AbstractValidator<PhoneNumber>
    {
        public PhoneNumberValidator()
        {
            RuleFor(v => v.CountryCode)
                .MaximumLength(10).
                NotEmpty();
            RuleFor(v => v.Number)
                .MaximumLength(10).
                NotEmpty();
        }
    }
}
