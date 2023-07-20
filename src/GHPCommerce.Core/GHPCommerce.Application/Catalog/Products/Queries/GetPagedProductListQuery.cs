using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    
    public class GetPagedProductListQuery : ICommand<SyncPagedResult<ProductDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
}
