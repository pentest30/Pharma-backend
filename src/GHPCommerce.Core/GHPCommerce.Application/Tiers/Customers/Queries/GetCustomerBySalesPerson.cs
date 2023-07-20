using System;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomerBySalesPerson : ICommand<IEnumerable<CustomerDto>>
    {
      
    }
    public class GetCustomerBySalesPersonId : ICommand<IEnumerable<CustomerDtoV1>>
    {
        public Guid SalesPersonId { get; set; }
    }
    public class GetPagedCustomersForSupervisor : ICommand<SyncPagedResult<CustomerDtoV1>>
    {
        public Guid? SalesPersonId { get; set; }
        public SyncDataGridQuery GridQuery { get; set; }
        //public Guid Supervisor { get; set; }
    }
}
