using System;
using System.Collections.Generic;
using GHPCommerce.Application.Tiers.BankAccounts.DTOs;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Organizations.DTOs
{
    public class OrganizationDtoV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string DisabledReason { get; set; }
        public string Owner { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        //public short Activity { get; set; }
        public OrganizationActivity Activity { get; set; }
        public string OrganizationActivity { get; set; }

        public DateTime? EstablishmentDate { get; set; }
        public string OrganizationGroupCode { get; set; }

        public List<AddressDto> Addresses { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public List<BankAccountDto> BankAccounts { get; set; }
        public List<EmailDto> Emails { get; set; }
        public bool ECommerce { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
    }
    
}
