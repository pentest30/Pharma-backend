using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Modules.Shared.Contracts.PreparationOrder.Queries
{
    public class GetNotPrintedOrders : ICommand<List<Guid>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
 
    }
}