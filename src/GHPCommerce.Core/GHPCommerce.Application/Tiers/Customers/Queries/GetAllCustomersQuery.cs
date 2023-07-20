using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetAllCustomersQuery : ICommand<List<CustomerDto>>
    {
        
    }
    public class GetAllCustomersQueryHandler : ICommandHandler<GetAllCustomersQuery, List<CustomerDto>> 
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetAllCustomersQueryHandler(IRepository<Customer, Guid> customerRepository, ICurrentOrganization currentOrganization)
        {
            _customerRepository = customerRepository;
            _currentOrganization = currentOrganization;
        }
       
        public async  Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Supplier.OrganizationId == orgId);
            var result =await query.Select(x => new CustomerDto
            {
                Id = (Guid)x.CustomerId,
                CustomerGroup = x.CustomerGroup,
                OrganizationId = x.Customer.OrganizationId,
                Code = x.Code,
                Name = x.Customer.Organization.Name,
            }).ToListAsync(cancellationToken: cancellationToken);
                

            return  result.Distinct().ToList();
        }
    }
}