using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using System;

namespace GHPCommerce.Application.Catalog.PickingZones.DTOs
{
    public class PickingZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public ZoneType ZoneType { get; set; }
        public Guid ZoneGroupId { get; set; }
        public string GroupName { get; set; }
        public ZoneGroupDto ZoneGroup { get; set; }

    }

}
