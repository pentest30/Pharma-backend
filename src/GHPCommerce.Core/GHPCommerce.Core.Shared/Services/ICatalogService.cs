using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;

namespace GHPCommerce.Core.Shared.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<CatalogDto>> GetCatalogsAsync();
    }
}
