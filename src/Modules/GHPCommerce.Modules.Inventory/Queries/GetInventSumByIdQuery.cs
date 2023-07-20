using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Inventory.DTOs;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetInventSumByIdQuery : ICommand<InventSumDto>
    {
        public Guid Id { get; set; }
    }
}