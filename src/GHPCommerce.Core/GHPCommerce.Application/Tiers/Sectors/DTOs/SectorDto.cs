using System;

namespace GHPCommerce.Application.Tiers.Sectors.DTOs
{
    public class SectorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int ExternalId { get; set; }
        public string Organization { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
