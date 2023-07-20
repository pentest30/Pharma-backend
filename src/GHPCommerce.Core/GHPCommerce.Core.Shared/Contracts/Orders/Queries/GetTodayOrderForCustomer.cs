using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class HasOrderToday : ICommand<bool>
    {
        public DateTime Date { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalesPersonId { get; set; }

    }
}
