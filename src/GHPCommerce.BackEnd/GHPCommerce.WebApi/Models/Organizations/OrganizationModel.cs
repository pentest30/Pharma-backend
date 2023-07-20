using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Application.Catalog.Manufacturers.Commands;
using GHPCommerce.Application.Tiers.Organizations.Commands;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.WebApi.Models.Organizations
{
    public class OrganizationModel
    {
        public Guid Id { get; set; }
       
        public string Name { get; set; }
        public string Code { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string DisabledReason { get; set; }
        public string Owner { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public short Activity { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<User> UserAccounts { get; set; }
        public List<EmailModel> Emails { get; set; }
        public Boolean ECommerce { get; set; }
    }

    public class OrganizationModelConfigMapping : Profile
    {
        public OrganizationModelConfigMapping()
        {
            CreateMap<OrganizationModel, CreateOrganizationCommand>()
                .ForMember(x=>x.Activity , o=> o.MapFrom(c=>(OrganizationActivity)c.Activity))
                .ReverseMap();
            CreateMap<OrganizationModel, UpdateOrganizationCommand>()
                .ForMember(x => x.Activity, o => o.MapFrom(c => (OrganizationActivity)c.Activity))
                .ReverseMap();
        }
    }

    public class OrganizationModelValidator : AbstractValidator<OrganizationModel>
    {
        private readonly ICommandBus _commandBus;

        public OrganizationModelValidator(ICommandBus commandBus)
        {
            _commandBus = commandBus;

            
            RuleFor(v => v.Name)
                .MaximumLength(200).NotEmpty()
                .MustAsync(NameMustBeUnique)
                .WithMessage("Le Nom doit être unique");
            RuleFor(v => v.Code)
                .MaximumLength(200)
                .NotEmpty();

            RuleFor(v => v.NIF)
                .MaximumLength(200).NotEmpty();
            RuleFor(v => v.NIS)
                .MaximumLength(200).NotEmpty();
            RuleFor(v => v.RC)
                .MaximumLength(200).NotEmpty();
            RuleFor(v => v.AI)
                .MaximumLength(200).NotEmpty();
            RuleForEach(x => x.Addresses)
                .SetValidator(new AddressValidator());
            RuleForEach(x => x.PhoneNumbers)
                .SetValidator(new PhoneNumberValidator());
        }

        private async Task<bool> NameMustBeUnique(OrganizationModel org, string v, CancellationToken token)
        {
            return await _commandBus.SendAsync(new OrganizationUniqueQuery {Id = org.Id, Name = org.Name}, token);
        }
    }
}
