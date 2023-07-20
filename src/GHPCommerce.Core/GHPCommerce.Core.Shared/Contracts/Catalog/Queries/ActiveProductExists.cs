using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class ActiveProductExists:ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}
