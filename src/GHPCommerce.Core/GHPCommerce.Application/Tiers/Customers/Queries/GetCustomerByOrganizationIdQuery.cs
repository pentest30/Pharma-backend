using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomerByOrganizationIdQueryV2 : ICommand<Guid>
    {
        public Guid OrganizationId { get; set; }
    }
}