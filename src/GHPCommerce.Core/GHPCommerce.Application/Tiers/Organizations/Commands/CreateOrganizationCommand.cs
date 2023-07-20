using System;
using System.Collections.Generic;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Organizations.Commands
{
    public class CreateOrganizationCommand : ICommand<ValidationResult>
    {
        public CreateOrganizationCommand()
        {
            Addresses = new List<Address>();
            PhoneNumbers = new List<PhoneNumber>();
            BankAccounts = new List<BankAccount>();
            UserAccounts = new List<User>();
            Emails = new List<EmailModel>();
            OrganizationStatus = OrganizationStatus.Active;

        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string DisabledReason { get; set; }
        public string Owner { get; set; }
        public string Code { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public OrganizationActivity Activity { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<User> UserAccounts { get; set; }
        public List<EmailModel> Emails { get; set; }
        public Boolean ECommerce { get; set; }
    }
    public class CreateOrganizationCommandConfigMapping : Profile
    {
        public CreateOrganizationCommandConfigMapping()
        {
            CreateMap<CreateOrganizationCommand, Organization>()
                .ForMember(x=>x.OrganizationGroupCode, o=> o.MapFrom(i=>i.Code))
                .ReverseMap();

        }
    }
}
