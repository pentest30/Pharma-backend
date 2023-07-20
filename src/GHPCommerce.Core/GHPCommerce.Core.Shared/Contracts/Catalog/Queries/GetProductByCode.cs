using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductByCode: ICommand<ProductDtoV3>
    {
        public string CodeProduct { get; set; }
    }
}
