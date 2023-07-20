using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;

namespace GHPCommerce.Modules.Shared.Contracts.PreparationOrder.Queries
{
    public class GetStateBlByOrderQuery : ICommand<List<PreparationOrderDtoV5>>
    {
        public Guid OrderId { get; set; }

    }
}
