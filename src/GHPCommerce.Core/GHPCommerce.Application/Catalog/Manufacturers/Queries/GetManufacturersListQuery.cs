using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Manufacturers.Queries
{
    public class GetManufacturersListQuery : CommonListQuery, ICommand<PagingResult<ManufacturerDto>>
    {
        public GetManufacturersListQuery(string tem, string sort, int page, int pageSize) : base(tem, sort, page, pageSize)
        {
        }
    }
}
