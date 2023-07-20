using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.Entities;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Queries.Consolidation
{
    public class GetConsolidationOrderByIdQuery : ICommand<ConsolidationOrder>
    {
        public Guid Id { get; set; }

    }
}
