using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class ChangeCustomerStatusCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }

    }
}
