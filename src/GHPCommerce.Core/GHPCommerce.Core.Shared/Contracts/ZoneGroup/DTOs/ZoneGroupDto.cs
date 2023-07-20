using System;

namespace GHPCommerce.Core.Shared.Contracts.ZoneGroup.DTOs
{
    public class ZoneGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Printer { get; set; }

    }
}
