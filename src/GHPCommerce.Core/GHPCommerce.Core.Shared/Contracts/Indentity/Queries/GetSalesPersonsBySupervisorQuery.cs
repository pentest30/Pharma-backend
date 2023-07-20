using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Indentity.Queries
{
    public class GetSalesPersonsBySupervisorQuery : ICommand<IEnumerable<Guid>>
    {
        public Guid Id { get; set; }
    }
}