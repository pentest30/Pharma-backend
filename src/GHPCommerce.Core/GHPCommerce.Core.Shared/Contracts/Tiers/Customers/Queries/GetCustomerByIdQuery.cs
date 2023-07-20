using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries
{
    public class GetCustomerByIdQuery : ICommand<CustomerDtoV1>
    {
        public Guid Id { get; set; }

    }
}
