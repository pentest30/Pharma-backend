using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Organization.Queries
{
    public class GetECommerceOrganizationIdsQuery : ICommand<IEnumerable<Guid>>
    {
    }
}
