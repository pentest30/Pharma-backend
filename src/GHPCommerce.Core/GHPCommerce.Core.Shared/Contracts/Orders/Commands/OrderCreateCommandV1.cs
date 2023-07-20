using System;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class OrderCreateCommandV1 : ICommand<AtomOrderContract>
    {
       
      
        public AtomOrderContract Order { get; set; }
    }
}