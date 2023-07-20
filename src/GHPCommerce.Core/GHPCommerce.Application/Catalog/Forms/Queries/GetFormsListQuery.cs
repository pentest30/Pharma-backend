using GHPCommerce.Application.Catalog.Forms.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Forms.Queries
{
    public class GetFormsListQuery : CommonListQuery, ICommand<PagingResult<FormDto>>
    {
        public GetFormsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}
