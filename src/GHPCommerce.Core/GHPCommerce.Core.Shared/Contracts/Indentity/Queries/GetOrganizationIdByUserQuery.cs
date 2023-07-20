using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Indentity.Queries
{
    public class GetOrganizationIdByUserQuery :ICommand<Guid?>
    {
        public Guid UserId { get; set; }
    }
}