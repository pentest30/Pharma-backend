using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Modules.Inventory.DTOs;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetStockForPreparation : ICommand<List<InventSumDto>>
    {
        public Guid SupplierId { get; set; }
        public string ProductCode { get; set; }
        public string  ZoneName { get; set; }

    }
}
