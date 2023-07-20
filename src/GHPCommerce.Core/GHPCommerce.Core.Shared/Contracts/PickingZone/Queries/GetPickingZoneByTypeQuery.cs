using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.PickingZone.Queries
{
    public class GetPickingZoneByTypeQuery : ICommand<PickingZoneDtoV1>
    {
        public ZoneType ZoneType { get; set; }

    }
}
