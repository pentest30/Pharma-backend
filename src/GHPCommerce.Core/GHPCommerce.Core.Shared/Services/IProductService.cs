using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Core.Shared.Services
{
    public interface IProductService
    {
        Task<PagingResult<ProductDtoV3>> GetListOfProductsAsync(Guid catalogId, int page, int pageSize);
    }
}