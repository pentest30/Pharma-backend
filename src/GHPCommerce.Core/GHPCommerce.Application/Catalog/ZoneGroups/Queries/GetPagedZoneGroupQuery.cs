using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Queries
{
    public class GetPagedZoneGroupQuery : ICommand<SyncPagedResult<ZoneGroupDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
    }
}
