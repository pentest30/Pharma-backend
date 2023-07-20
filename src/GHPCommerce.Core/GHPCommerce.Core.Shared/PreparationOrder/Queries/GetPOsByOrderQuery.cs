using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.PreparationOrder.Queries
{
    
    public class GetPOsByOrderQuery :  ICommand<List<PreparationOrderItemDtoV1>>
    {
        public Guid OrderId { get; set; }
    }
}