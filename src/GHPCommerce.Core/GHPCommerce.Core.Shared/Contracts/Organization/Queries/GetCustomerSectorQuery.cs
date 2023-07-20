using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Organization.Queries
{
    public class GetCustomerSectorQuery : ICommand<string>
    {
        public Guid OrganizationId{ get; set; }
        public Guid SupplierId { get; set; }
    }
}