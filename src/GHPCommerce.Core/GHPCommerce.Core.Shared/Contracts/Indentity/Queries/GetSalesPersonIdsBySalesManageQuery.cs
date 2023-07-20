using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Indentity.Queries
{
    /// <summary>
    /// this query returns a list of sales persons ids for a sale manager
    /// </summary>
    public class GetSalesPersonIdsBySalesManageQuery : ICommand<IEnumerable<Guid>>
    {
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
    }
}
