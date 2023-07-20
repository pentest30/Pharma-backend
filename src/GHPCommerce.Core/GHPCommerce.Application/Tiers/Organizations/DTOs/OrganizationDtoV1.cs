using System;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Organizations.DTOs
{
    public class OrganizationDtoV1
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string DisabledReason { get; set; }
        public string Owner { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public string OrganizationState { get; set; }
        public OrganizationActivity Activity { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public string EstablishmentDateShort { get; set; }
        public string OrganizationActivity { get; set; }
        public string OrganizationGroupCode { get; set; }
    }

   
}
