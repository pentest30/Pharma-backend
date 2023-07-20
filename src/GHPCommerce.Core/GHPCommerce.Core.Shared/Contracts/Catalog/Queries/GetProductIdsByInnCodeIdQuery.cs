using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductIdsByInnCodeIdQuery : ICommand<IEnumerable<Guid>>
    {
        public Guid InnCodeId { get; set; }
    }
}
