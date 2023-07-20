using GHPCommerce.Core.Shared.Contracts.ZoneGroup.DTOs;
using System;

namespace GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs
{
    public class PickingZoneDtoV1
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid ZoneGroupId { get; set; }
        public ZoneGroupDto ZoneGroup { get; set; }
    }
    

}
