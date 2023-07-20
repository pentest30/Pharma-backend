using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Dosages.Queries
{
    public class GetDosagesListQuery :CommonListQuery, ICommand<PagingResult<DosageDto>>
    {
        public GetDosagesListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}
