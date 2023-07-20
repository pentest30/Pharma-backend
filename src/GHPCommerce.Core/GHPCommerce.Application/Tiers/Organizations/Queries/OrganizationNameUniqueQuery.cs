using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class OrganizationUniqueQuery :ICommand<bool>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RC { get; set; }
    }
}
