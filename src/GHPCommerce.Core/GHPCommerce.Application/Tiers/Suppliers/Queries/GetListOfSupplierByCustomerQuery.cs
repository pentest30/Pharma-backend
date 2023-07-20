using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetListOfSupplierByCustomerQuery : ICommand<List<SupplierDto>>
    {
        public Guid CustomerId { get; set; }
    }

    public class GetListOfSupplierByCustomerQueryHandler : ICommandHandler<GetListOfSupplierByCustomerQuery, List<SupplierDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        public GetListOfSupplierByCustomerQueryHandler(IMapper mapper, 
            IRepository<Customer, Guid> customerRepository,
            IRepository<Supplier, Guid> supplierRepository,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            ICurrentOrganization currentOrganization)
        {
            _mapper = mapper;
            _customerRepository = customerRepository;
            _supplierRepository = supplierRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
        public async Task<List<SupplierDto>> Handle(GetListOfSupplierByCustomerQuery request, CancellationToken cancellationToken)
        {
            var query =  await _supplierRepository.Table
                .Include(c => c.SupplierCustomers)
                .SelectMany(c => c.SupplierCustomers)
                .Where(c => c.CustomerId == request.CustomerId)
                .Select(c => new SupplierDto
                {
                    Id = c.SupplierId.Value,
                    OrganizationId = c.Supplier.OrganizationId,
                    Name = c.Supplier.Organization.Name
                    
                })
                .ToListAsync(cancellationToken: cancellationToken);
                return query;
        }
    }
}