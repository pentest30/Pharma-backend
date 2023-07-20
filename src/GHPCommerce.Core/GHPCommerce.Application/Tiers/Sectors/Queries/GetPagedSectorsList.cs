using GHPCommerce.Application.Tiers.Sectors.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{

    public class GetPagedSectorsList : ICommand<SyncPagedResult<SectorDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }

}
