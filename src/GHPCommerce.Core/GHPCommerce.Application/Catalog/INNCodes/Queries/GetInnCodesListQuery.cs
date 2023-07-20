using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.INNCodes.Queries
{
    public class GetInnCodesListQuery :CommonListQuery, ICommand<PagingResult<InnCodeDto>> 
    {
        public GetInnCodesListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}
