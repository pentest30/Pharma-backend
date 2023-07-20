using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.Entities;

namespace GHPCommerce.Modules.PreparationOrder.Queries.Consolidation
{
    public class GetConsolidationOrderByOrderIdQuery : ICommand<ConsolidationOrder>
    {
        public Guid OrderId { get; set; }
    }
}