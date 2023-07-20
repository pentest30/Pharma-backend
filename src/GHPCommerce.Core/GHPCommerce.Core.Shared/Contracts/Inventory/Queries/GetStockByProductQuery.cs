using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Queries
{
    /// <summary>
    /// this query returns Available quantity of  product in vendor's stock for b2b customer 
    /// </summary>
    public class GetStockByProductQuery : ICommand<List<InventSumDtoV1>>
    {
        public Guid SupplierId { get; set; }
        public List<string> ProductCodes { get; set; }
    }
}
