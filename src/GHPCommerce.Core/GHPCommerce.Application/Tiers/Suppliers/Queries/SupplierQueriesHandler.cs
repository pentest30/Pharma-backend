using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class SupplierQueriesHandler : ICommandHandler<GetSuppliersListQuery, PagingResult<OrganizationDtoV1>>,
        ICommandHandler<GetSuppliersForB2BCustomerListQuery, IEnumerable<OrganizationDto>>,
        ICommandHandler<GetPagedSuppliersQuery, SyncPagedResult<SupplierDto>>,
        ICommandHandler<GetSupplierByIdQuery, SupplierDto>,
        ICommandHandler<GetByIdOfSupplierQuery, SupplierDto>


    {
        private readonly IMapper _mapper;
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public SupplierQueriesHandler(IMapper mapper, 
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
        public async Task<PagingResult<OrganizationDtoV1>> Handle(GetSuppliersListQuery request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!organizationId.HasValue)
                throw new InvalidOperationException("");
            var total = await _customerRepository.Table
                .Where(x => x.OrganizationId == organizationId)
                .Include(x => x.SupplierCustomers)
                .SelectMany(x => x.SupplierCustomers)
                .CountAsync(cancellationToken: cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Supplier.Organization.Name " + request.SortDir : request.SortProp + " " + request.SortDir;
            var suppliersQuery = _customerRepository.Table
                .Where(x => x.OrganizationId == organizationId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Supplier.Organization")
                .SelectMany(x => x.SupplierCustomers)
                .Select(e=> e.Supplier.Organization)
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize);

            if (!string.IsNullOrEmpty(request.Term))
            {
                // ReSharper disable once ComplexConditionExpression
                suppliersQuery = suppliersQuery.Where(x => x.Name.ToLower().Contains(request.Term.ToLower())
                                                           || x.AI.ToLower().Contains(request.Term.ToLower())
                                                           || x.NIF.ToLower().Contains(request.Term.ToLower())
                                                           || x.NIS.ToLower().Contains(request.Term.ToLower())
                                                           || x.RC.ToLower().Contains(request.Term.ToLower()));
            }

            var data = _mapper.Map<IEnumerable<OrganizationDtoV1>>(await suppliersQuery.ToListAsync(cancellationToken: cancellationToken));
            return new PagingResult<OrganizationDtoV1> { Data = data, Total = total };
            

        }

        public async Task<IEnumerable<OrganizationDto>> Handle(GetSuppliersForB2BCustomerListQuery request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();



            if (!organizationId.HasValue)
                throw new InvalidOperationException("");
            var query =  _customerRepository.Table;

            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);

            if (currentUser.UserRoles.All(x => x.Role.Name == "Admin"))
                query = query.Where(x => x.OrganizationId == organizationId);
            
            var result = await query.Include(x => x.SupplierCustomers)
            .Include("SupplierCustomers.Supplier")
            .Include("SupplierCustomers.Supplier.Organization")
            .SelectMany(x => x.SupplierCustomers)
            .Select(e => new OrganizationDto {Id = e.Supplier.OrganizationId, Name = e.Supplier.Organization.Name})
            .ToListAsync(cancellationToken: cancellationToken);
            return result;
        }

        public async Task<SyncPagedResult<SupplierDto>> Handle(GetPagedSuppliersQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);

            var query = _customerRepository.Table
                .Where(c => c.OrganizationId == currentUser.OrganizationId)
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Supplier.Organization")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers);
                
            if (request.GridQuery.Where != null /*&& request.GridQuery.Where.Predicates.Any()*/)
            {
                foreach (var wherePredicate in request.GridQuery.Where[0].Predicates)
                {
                    if (wherePredicate.Field == "name")
                        query = query.Where(x=> x.Supplier.Organization.Name.Contains( wherePredicate.Value.ToString()));

                    else if (wherePredicate.Field == "organizationGroupCode")
                        query = query.Where($"Supplier.Organization.OrganizationGroupCode.Contains(@0)", wherePredicate.Value.ToString());
                    else if (wherePredicate.Field == "sector")
                        query = query.Where($"Sector.Name.Contains(@0)", wherePredicate.Value.ToString());
                    
                    else if (wherePredicate.Field == "paymentDeadline")
                    {
                        if (int.TryParse(wherePredicate.Value.ToString(), out int value))
                        {
                            query = query.Where(x => x.PaymentDeadline == value);
                        }
                    }else if(wherePredicate.Field == "activity")
                    {
                        OrganizationActivity activity = (OrganizationActivity) Enum.Parse(typeof(OrganizationActivity),wherePredicate.Value.ToString());
                        query = query.Where(x => x.Supplier.Organization.Activity.HasFlag(activity)); 
                    }
                    else if(wherePredicate.Field == "nif" || wherePredicate.Field == "nis" || wherePredicate.Field == "ai" || wherePredicate.Field == "rc")
                        query = query.Where($"Supplier.Organization.{wherePredicate.Field.ToUpper()}.Contains(@0)", wherePredicate.Value.ToString());


                }

            }
            
            query = query.DynamicWhereQuery(request.GridQuery);
            var total = await query.CountAsync(cancellationToken);
            var result = query.OrderBy(c => c.Supplier.Organization.Name)
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take)
                .Select(x => new SupplierDto
                {
                    EntityId = x.Id,
                    OrganizationGroupCode = x.Supplier.Organization.OrganizationGroupCode,
                    Id = (Guid)x.SupplierId,
                    CustomerGroup = x.CustomerGroup,
                    OrganizationId = x.Supplier.OrganizationId,
                    Code = x.Code,
                    Name = x.Supplier.Organization.Name,
                    Debt = x.Dept,
                    ConventionType = x.ConventionType,
                    PaymentDeadline = x.PaymentDeadline,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    SalesGroup = x.SalesGroup,
                    LimitCredit = x.LimitCredit,
                    Sector = x.Sector.Name,
                    SectorCode = x.Sector.Code,
                    PaymentMode = x.PaymentMode,
                    PaymentMethod = x.PaymentMethod,
                    MonthlyObjective = x.MonthlyObjective,
                    CustomerStatus = x.CustomerState == CustomerState.Active ? "Actif" : "Bloqué",
                    AddressDto = new AddressDto(x.Supplier.Organization.Addresses.FirstOrDefault(ax => ax.Main)),
                    PhoneNumber = x.Supplier.Organization.PhoneNumbers.Any()
                        ? x.Supplier.Organization.PhoneNumbers.FirstOrDefault().CountryCode + " " + x.Supplier.Organization.PhoneNumbers.FirstOrDefault().Number
                        : "",
                    NIF = x.Supplier.Organization.NIF,
                    NIS = x.Supplier.Organization.NIS,
                    AI = x.Supplier.Organization.AI,
                    Activity = x.Supplier.Organization.Activity,
                    RC = x.Supplier.Organization.RC,
                    CustomerState = x.CustomerState,
                    EstablishmentDateShort = x.Supplier.Organization.EstablishmentDate.Value.ToShortDateString()
                });
            return new SyncPagedResult<SupplierDto>
            { Count = total, Result = (await result.ToListAsync(cancellationToken: cancellationToken)) };
        }

        public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!organizationId.HasValue)
                throw new InvalidOperationException("");
          
            var query = await _customerRepository.Table
                .Where(c => c.OrganizationId == organizationId.Value)
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Supplier.Organization")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x=>x.Supplier.OrganizationId == request.SupplierId).Select(x => new SupplierDto
                {
                    EntityId = x.Id,
                    OrganizationGroupCode = x.Supplier.Organization.OrganizationGroupCode,
                    Id = (Guid)x.SupplierId,
                    CustomerGroup = x.CustomerGroup,
                    OrganizationId = x.Supplier.OrganizationId,
                    Code = x.Code,
                    Name = x.Supplier.Organization.Name,
                    Debt = x.Dept,
                    ConventionType = x.ConventionType,
                    PaymentDeadline = x.PaymentDeadline,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    SalesGroup = x.SalesGroup,
                    LimitCredit = x.LimitCredit,
                    Sector = x.Sector.Name,
                    SectorCode = x.Sector.Code,
                    PaymentMode = x.PaymentMode,
                    PaymentMethod = x.PaymentMethod,
                    MonthlyObjective = x.MonthlyObjective,
                    CustomerStatus = x.CustomerState == CustomerState.Active ? "Actif" : "Bloqué",
                    AddressDto = new AddressDto(x.Supplier.Organization.Addresses.FirstOrDefault(ax => ax.Main)),
                    PhoneNumber = x.Supplier.Organization.PhoneNumbers.Any()
                        ? x.Supplier.Organization.PhoneNumbers.FirstOrDefault().CountryCode + " " + x.Supplier.Organization.PhoneNumbers.FirstOrDefault().Number
                        : ""
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);;
            return query;
        }

        public async Task<SupplierDto> Handle(GetByIdOfSupplierQuery request, CancellationToken cancellationToken)
        {
            
            var query = await _supplierRepository.Table
                .Where(c => c.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return _mapper.Map<SupplierDto>(query);
        }
      }
}
