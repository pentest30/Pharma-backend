using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.PickingZone.Queries
{
    public class GetPickingZoneByNameQuery : ICommand<PickingZoneDtoV1>
    {
        public string ZoneName { get; set; }

    }
}
