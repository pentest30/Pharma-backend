using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Queries
{
    public class GetListOfProductForQuotaQuery : ICommand<IEnumerable<InventSumQuotaDto>>
    {
        
    }
}