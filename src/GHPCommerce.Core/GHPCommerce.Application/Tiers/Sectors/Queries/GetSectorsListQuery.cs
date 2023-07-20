using GHPCommerce.Application.Tiers.Sectors.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{
    public class GetSectorsListQuery : CommonListQuery, ICommand<PagingResult<SectorDto>>
    {
        public GetSectorsListQuery(string term, string sort, int page, int pageSize) : base(term, sort, page, pageSize)
        {
            
        }
    }
}
