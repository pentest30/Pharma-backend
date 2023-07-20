using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.PickingZones.Queries
{
    public class GetPickingZonesListQuery : CommonListQuery, ICommand<PagingResult<PickingZoneDto>>
    {
        public GetPickingZonesListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}
