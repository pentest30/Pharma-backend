using System;

namespace GHPCommerce.Application.Catalog.ZoneGroups.DTOs
{
    public class ZoneGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Printer { get; set; }

    }
}
