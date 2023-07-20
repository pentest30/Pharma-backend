using System.Collections.Generic;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PickingZones.Queries
{
    public class GetAllPickingZonesQuery : ICommand<IEnumerable<PickingZoneDto>>
    {
    }
}
