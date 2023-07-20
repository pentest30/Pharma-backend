using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Core.Shared.Contracts.Organization.DTOs;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class OrganizationQueriesHandler :
        ICommandHandler<GetOrganizationListQuery, PagingResult<OrganizationDtoV1>>,
        ICommandHandler<GetAllOrganizationsQuery, IEnumerable<OrganizationDto>>,
        ICommandHandler<OrganizationUniqueQuery, Boolean>,
        ICommandHandler<GetOrganizationByIdQuery, OrganizationDtoV2>,
        ICommandHandler<GetWholesaleByIdQuery, WholeSalerDto>,
        ICommandHandler<GetWholesaleByIdQueryV2, WholeSalerDto>,
        ICommandHandler<GetECommerceOrganizationIdsQuery, IEnumerable<Guid>>,
        ICommandHandler<GetPharmacistQuery, IEnumerable<PharmacistDto>>,
        ICommandHandler<GetOrganizationByNameQuery, OrganizationDto>,
        ICommandHandler<GetCustomerOrganizationIdsQuery, IEnumerable<Guid>>,
        ICommandHandler<PagedOrganizationsQuery, SyncPagedResult<OrganizationDtoV1>>,
        ICommandHandler<GetCustomerSectorQuery, string>

    {
        private readonly IMapper _mapper;
        private readonly IRepository<Organization, Guid> _organizationRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ICache _redisCache;

        public OrganizationQueriesHandler(IMapper mapper, IRepository<Organization, Guid> organizationRepository,
            IRepository<Supplier, Guid> supplierRepository,IRepository<Customer, Guid> customerRepository, ICache redisCache)
        {
            _mapper = mapper;
            _organizationRepository = organizationRepository;
            _supplierRepository = supplierRepository;
            _customerRepository = customerRepository;
            _redisCache = redisCache;
        }

        public async Task<PagingResult<OrganizationDtoV1>> Handle(GetOrganizationListQuery request,
            CancellationToken cancellationToken)
        {
            var total = await _organizationRepository.Table.CountAsync(cancellationToken);
            // ReSharper disable once ComplexConditionExpression
            string orderQuery = string.IsNullOrEmpty(request.SortProp)
                ? "Name " + request.SortDir
                : request.SortProp + " " + request.SortDir;

            var query = _organizationRepository
                .Table
                .OrderBy(orderQuery)
                .Where(x => string.IsNullOrEmpty(request.Term)
                            || x.Name.ToLower().Contains(request.Term.ToLower())
                            || x.AI.ToLower().Contains(request.Term.ToLower())
                            || x.NIF.ToLower().Contains(request.Term.ToLower())
                            || x.NIS.ToLower().Contains(request.Term.ToLower())
                            || x.RC.ToLower().Contains(request.Term.ToLower()))
                .Paged(request.Page, request.PageSize);
            var data = _mapper.Map<IEnumerable<OrganizationDtoV1>>(await query.ToListAsync(cancellationToken));
            return new PagingResult<OrganizationDtoV1> {Data = data, Total = total};
        }

        public async Task<IEnumerable<OrganizationDto>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
        {
            var query = await _organizationRepository
                .Table
                .Select(x => new OrganizationDto { Name = x.Name, Id = x.Id })
                // ReSharper disable once TooManyChainedReferences
                .ToListAsync(cancellationToken);
            return query;
        }

        public async Task<bool> Handle(OrganizationUniqueQuery request, CancellationToken cancellationToken)
        {
            var organizationExistence =
                // ReSharper disable once ComplexConditionExpression
                await _organizationRepository.Table.AnyAsync(x => x.Name == request.Name && x.Id != request.Id,
                    cancellationToken);
            return !organizationExistence;
        }

        public async Task<OrganizationDtoV2> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
        {
            var query = await _organizationRepository
                .Table
                .AsNoTracking()
                .Include(x=>x.Addresses)
                .Include(x => x.BankAccounts)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.Emails)
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            return _mapper.Map<OrganizationDtoV2>(query);
        }

        public async Task<WholeSalerDto> Handle(GetWholesaleByIdQuery request, CancellationToken cancellationToken)
        {
             var organizationQuery = _organizationRepository.Table
                .Where(x => x.Id == request.SupplierOrganizationId)
                .Include(x => x.Supplier.SupplierCustomers)
                .ThenInclude(s=>s.Customer)
                .ThenInclude(c=>c.Organization)
                .SelectMany(x => x.Supplier.SupplierCustomers)
                .Where(x => x.Customer.OrganizationId == request.CustomerOrganizationId)
                .Select(x => new WholeSalerDto {Name = x.Supplier.Organization.Name, DefaultSalesPersonId = x.DefaultSalesPerson});
            return  await organizationQuery.FirstOrDefaultAsync(cancellationToken);

        }

        public async Task<IEnumerable<Guid>> Handle(GetECommerceOrganizationIdsQuery request, CancellationToken cancellationToken)
        {
            return await _organizationRepository
                .Table
                .Where(x => x.ECommerce).Select(x => x.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<PharmacistDto>> Handle(GetPharmacistQuery request, CancellationToken cancellationToken)
        {
            var count = await _organizationRepository
                .Table
                .CountAsync(p => p.Activity == OrganizationActivity.Pharmacist, cancellationToken);

            var organizationQuery = await _redisCache.GetAsync<IEnumerable<PharmacistDto>>("_pharmacists", token: cancellationToken);
            if (organizationQuery != null && organizationQuery.Any() && count == organizationQuery.Count())
                return organizationQuery;
            organizationQuery = await _organizationRepository
                .Table.Include(x => x.Addresses)
                .Where(p => p.Activity == OrganizationActivity.Pharmacist && p.OrganizationStatus!= OrganizationStatus.Blocked)
                .Select(x => new PharmacistDto
                    {Email = x.Emails.FirstOrDefault().Email,Id =x.Id, Name = x.Name, Address = new AddressDto(x.Addresses.FirstOrDefault(a => a.Main))})
                .ToListAsync(cancellationToken: cancellationToken);
            await _redisCache.AddOrUpdateAsync<PharmacistDto>("_pharmacists", organizationQuery, cancellationToken);
            return organizationQuery;
        }

        public Task<WholeSalerDto> Handle(GetWholesaleByIdQueryV2 request, CancellationToken cancellationToken)
        {
            var organizationQuery = _organizationRepository.Table
                .Where(x => x.Id == request.SupplierOrganizationId)
                .Include(x=>x.Emails)
                .Select(x => new WholeSalerDto { Name = x.Name, Email = x.Emails.FirstOrDefault().Email })
                .FirstOrDefaultAsync(cancellationToken);
            return organizationQuery;


        }

        public async Task<OrganizationDto> Handle(GetOrganizationByNameQuery request, CancellationToken cancellationToken)
        {
            var query = await _organizationRepository
                .Table
                .Where(x=>x.Name.ToLower() == request.Name.ToLower())
                .Select(x => new OrganizationDto { Name = x.Name, Id = x.Id })
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(cancellationToken);
            return query;
        }

        public async Task<IEnumerable<Guid>> Handle(GetCustomerOrganizationIdsQuery request, CancellationToken cancellationToken)
        {
            var organizationQuery = _supplierRepository.Table.Where(x => x.OrganizationId == request.SupplierId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .SelectMany(x => x.SupplierCustomers)
                .Select(x => x.Customer.Id);

            return await organizationQuery.ToArrayAsync(cancellationToken: cancellationToken);
           
        }

        public async Task<SyncPagedResult<OrganizationDtoV1>> Handle(PagedOrganizationsQuery request, CancellationToken cancellationToken)
        {
            var query = _organizationRepository.Table 
                .DynamicWhereQuery(request.GridQuery);
            var total = await query.CountAsync(cancellationToken);
            var result = query.OrderBy(c => c.Name)
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take);
            var data = _mapper.Map<IEnumerable<OrganizationDtoV1>>(await result.ToListAsync(cancellationToken));
            return new SyncPagedResult<OrganizationDtoV1> { Count = total, Result = data };
        }

        public async Task<string> Handle(GetCustomerSectorQuery request, CancellationToken cancellationToken)
        {
            var query = await _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Customer.OrganizationId == request.OrganizationId &&
                            x.Supplier.OrganizationId == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return query?.Sector?.Code;

        }
    }
}
