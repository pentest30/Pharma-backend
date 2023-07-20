using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class 
        ToCacheCommand : ICommand<OrderDtoV2>
    {
        public Guid Id { get; set; }
    }
}
