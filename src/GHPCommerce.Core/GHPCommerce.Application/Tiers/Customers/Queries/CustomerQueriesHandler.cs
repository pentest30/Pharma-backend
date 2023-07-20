using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Invoices.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class CustomerQueriesHandler :
        ICommandHandler<GetPotentialCustomersQuery, IEnumerable<OrganizationDto>>,
        ICommandHandler<GetCustomersQuery, PagingResult<SupplierCustomerDto>>,
        ICommandHandler<GetSupplierCustomerByIdQuery, SupplierCustomerDto>,
        ICommandHandler<GetAllowedProductClassesQuery, IEnumerable<AllowedProductClass>>,
        ICommandHandler<GetCustomerBySalesPerson, IEnumerable<CustomerDto>>,
        ICommandHandler<GetCustomerByIdQuery, CustomerDtoV1>,
        ICommandHandler<GetCustomerByOrganizationIdQuery, CustomerDtoV1>,
        ICommandHandler<PagedCustomersBySalesPerson, SyncPagedResult<CustomerDto>>,
        ICommandHandler<GetCustomerForSalesPersonById, CustomerDto>,
        ICommandHandler<GetCustomerBySalesPersonDash, SyncPagedResult<CustomerDtoV2>>,
        ICommandHandler<GetCustomersBySalesPersonIdDashQuery, IEnumerable<CustomerDtoV2>>,
        ICommandHandler<GetCustomerByCodeQuery, CustomerDtoV1>,
        ICommandHandler<GetCustomerByCodeQueryV2, CustomerDtoV1>,
        ICommandHandler<GetCustomerBySalesPersonId, IEnumerable<CustomerDtoV1>>,
        ICommandHandler<GetPagedCustomersForSupervisor, SyncPagedResult<CustomerDtoV1>>,
        ICommandHandler<GetDailyDashboardBySalesPersonIdDashQuery, DashboardDto>,
        ICommandHandler<GetMonthlyDashboardBySalesPersonIdDashQuery, DashboardDto>,
        ICommandHandler<GetCustomerByCodeQueryV3, CustomerDtoV1>


    {

        private readonly IMapper _mapper;
        private readonly IRepository<Organization, Guid> _organizationRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        public CustomerQueriesHandler(IMapper mapper,
            IRepository<Organization, Guid> organizationRepository,
            IRepository<Supplier, Guid> supplierRepository,
            IRepository<Customer, Guid> customerRepository,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _mapper = mapper;
            _organizationRepository = organizationRepository;
            _supplierRepository = supplierRepository;
            _customerRepository = customerRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
        }

        public async Task<IEnumerable<OrganizationDto>> Handle(GetPotentialCustomersQuery request,
            CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!organizationId.HasValue) throw new InvalidOperationException("Vous n'avez pas l'autorisation d'accéder à cette ressource");

            // on ramène les clients de l'organisation en cours
            var customersIds = GetCustomersOfCurrentOrganizationQuery(organizationId);
            var orgs = await GetOrganizationsAsync(organizationId, customersIds);
            return orgs;
        }

        private async Task<List<OrganizationDto>> GetOrganizationsAsync(Guid? organizationId,
            IQueryable<Guid> organizationIds)
        {
            var organisationIdsList = await organizationIds.ToListAsync();
            var orgQuery = await _organizationRepository.Table.ToListAsync();
            var orgs = (from org in orgQuery
                        where org.Id != organizationId
                        && org.OrganizationStatus == OrganizationStatus.Active
                      && organisationIdsList.All(x => x != org.Id)
                        select new OrganizationDto { Id = org.Id, Name = org.Name, OrganizationGroupCode = org.OrganizationGroupCode }).ToList();
            return orgs;
        }

        private IQueryable<Guid> GetCustomersOfCurrentOrganizationQuery(Guid? organizationId)
        {
            var organizationQuery = _supplierRepository.Table.Where(x => x.OrganizationId == organizationId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .SelectMany(x => x.SupplierCustomers)
                .Select(x => x.Customer.OrganizationId);

            return organizationQuery;
        }

        public async Task<PagingResult<SupplierCustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!organizationId.HasValue) return null;
            var total = await _supplierRepository.Table
                .Where(x => x.OrganizationId == organizationId)
                .Include(x => x.SupplierCustomers)
                .SelectMany(x => x.SupplierCustomers)
                .CountAsync(cancellationToken: cancellationToken);

            string orderQuery = string.IsNullOrEmpty(request.SortProp)
                ? "Customer.Organization.Name " + request.SortDir
                : GetSortQuery(request.SortProp, request.SortDir);
            var supplier = _supplierRepository.Table
                .Where(x => x.OrganizationId == organizationId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.TaxGroup")
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .SelectMany(x => x.SupplierCustomers.Distinct())
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize);

            if (!string.IsNullOrEmpty(request.Term))
            {
                supplier = supplier.Where(x => x.Customer.Organization.Name.ToLower().Contains(request.Term.ToLower()));
            }

            var data = _mapper.Map<IEnumerable<SupplierCustomerDto>>(await supplier.ToListAsync(cancellationToken));
            return new PagingResult<SupplierCustomerDto> { Data = data.Distinct(), Total = total };
        }

        private string GetSortQuery(string requestSortProp, string requestSortDir)
        {
            return requestSortProp + " " + requestSortDir;

        }

        public async Task<SupplierCustomerDto> Handle(GetSupplierCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {

            var customer = _customerRepository.Table
                .Where(x => x.OrganizationId == request.CustomerOrganizationId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.PermittedProductClasses")
                .Include("SupplierCustomers.TaxGroup")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .Include("SupplierCustomers.Customer")
                .SelectMany(e => e.SupplierCustomers)
                .Where(x => x.CustomerId == request.Id);
            //.FirstOrDefaultAsync(cancellationToken);
            var manager = default(User);
            var salesPerson = default(User);
            var data = _mapper.Map<SupplierCustomerDto>(await customer.FirstOrDefaultAsync(cancellationToken));
            data.SectorCode = customer.FirstOrDefault()?.Sector?.Code;
            if (data.DefaultSalesPerson.HasValue)
            {
                salesPerson = await _commandBus.SendAsync(new GetUserQuery { Id = data.DefaultSalesPerson.Value, IncludeRoles = true }, cancellationToken);
                if (salesPerson?.ManagerId != default)
                    manager = await _commandBus.SendAsync(new GetUserQuery { Id = salesPerson.ManagerId.Value, IncludeRoles = true }, cancellationToken);
            }

            var organization = _mapper.Map<OrganizationDtoV2>(await _organizationRepository.Table
                         .AsNoTracking()
                         .Include(x => x.Addresses)
                         .Include(x => x.BankAccounts)
                         .Include(x => x.PhoneNumbers)
                         .Include(x => x.Emails)
                         // ReSharper disable once TooManyChainedReferences
                         .FirstOrDefaultAsync(x => x.Id == request.CustomerOrganizationId, cancellationToken));
            data.Addresses = organization.Addresses;
            data.DefaultSalesGroupName = manager?.UserName;
            data.PhoneNumbers = organization.PhoneNumbers;
            data.Emails = organization.Emails;
            data.BankAccounts = organization.BankAccounts;
            data.Activity = organization.OrganizationActivity;
            data.ECommerce = (organization.ECommerce) ? "Oui" : "Non";
            data.EstablishmentDate = organization.EstablishmentDate;
            data.DisabledReason = organization.DisabledReason;
            data.NIF = organization.NIF;
            data.NIS = organization.NIS;
            data.RC = organization.RC;
            data.AI = organization.AI;
            data.CreatedDateTime = organization.CreatedDateTime;
            data.CreatedBy = organization.CreatedBy;
            data.UpdatedBy = organization.UpdatedBy;
            data.UpdatedDateTime = organization.UpdatedDateTime;
            data.SalesPersonName = salesPerson?.UserName;
            
            return data;

        }

        public async Task<IEnumerable<AllowedProductClass>> Handle(GetAllowedProductClassesQuery request, CancellationToken cancellationToken)
        {
            var query = await _customerRepository.Table
                .Where(x => x.OrganizationId == request.CustomerOrganizationId)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.PermittedProductClasses")
                .SelectMany(x => x.SupplierCustomers)
                .FirstOrDefaultAsync(x => x.Supplier.OrganizationId == request.SupplierOrganizationId, cancellationToken);
            return query.PermittedProductClasses;

        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetCustomerBySalesPerson request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);

            var predicate = SharedCustomers(currentUser);
            var query = await _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(predicate)
                //  .Where(x=> string.IsNullOrEmpty(request.Name) || x.Customer.Organization.Name.ToLower().Contains(request.Name))
                .Where(x => x.CustomerState != CustomerState.BlockedForOrders
                            && x.CustomerState != CustomerState.Blocked 
                            && (!x.ActualSalesPerson.HasValue && x.DefaultSalesPerson == currentUser.Id || x.ActualSalesPerson == currentUser.Id))
                .Select(x => new CustomerDto
                {
                    Id = (Guid)x.CustomerId,
                    OrganizationGroupCode = x.Customer.Organization.OrganizationGroupCode,
                    OrganizationId = x.Customer.OrganizationId,
                    CustomerGroup = x.CustomerGroup,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    Debt = x.Dept,
                    ConventionType = x.ConventionType,
                    PaymentDeadline = x.PaymentDeadline,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    SalesGroup = x.SalesGroup,
                    LimitCredit = x.LimitCredit,
                    Sector = x.Sector.Name,
                    SectorCode = x.Sector.Code,
                    CustomerState = x.CustomerState,
                    PaymentMode = x.PaymentMode,
                    PaymentMethod = x.PaymentMethod,
                    CustomerStatus = x.CustomerState == CustomerState.Active ? "Actif" : "Bloqué",
                    MonthlyObjective = x.MonthlyObjective,
                    AddressDto = new AddressDto(x.Customer.Organization.Addresses.FirstOrDefault(ax => ax.Main)),
                    PhoneNumber = x.Customer.Organization.PhoneNumbers.Any() ? x.Customer.Organization.PhoneNumbers.FirstOrDefault().Number : ""

                })
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return query;
        }

        private static Expression<Func<SupplierCustomer, bool>> SharedCustomers(User currentUser)
        {
            var sharedCustomers = true;
            Expression<Func<SupplierCustomer, bool>> predicate = customer =>
                sharedCustomers && customer.Supplier.OrganizationId == currentUser.OrganizationId || customer.DefaultSalesPerson == currentUser.Id;
            return predicate;
        }

        public async Task<CustomerDtoV1> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _customerRepository.Table
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Sector")
                .Select(x => new CustomerDtoV1
                {

                    Name =x.Organization!=null?  x.Organization.Name:"",
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    Code =x.SupplierCustomers.FirstOrDefault()!=null? x.SupplierCustomers.FirstOrDefault().Code : "",
                    Sector =x.SupplierCustomers.FirstOrDefault()!=null &&x.SupplierCustomers.FirstOrDefault().Sector!=null? x.SupplierCustomers.FirstOrDefault().Sector.Name :"",
                    DeadLine =x.SupplierCustomers.FirstOrDefault()!=null ?x.SupplierCustomers.FirstOrDefault().PaymentDeadline:0,
                    SalesManager = x.SupplierCustomers.FirstOrDefault()!=null?x.SupplierCustomers.FirstOrDefault().SalesGroup :"",
                    SalesPersonId = x.SupplierCustomers.FirstOrDefault()!=null?x.SupplierCustomers.FirstOrDefault().DefaultSalesPerson: null


                }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        }

        public async Task<CustomerDtoV1> Handle(GetCustomerByOrganizationIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _customerRepository
                .Table
                  .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Sector")
                .Include("Organization.Addresses")
                .Select(x => new CustomerDtoV1
                {
                    Name = x.Organization.Name,
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    SupplierId = x.SupplierCustomers.FirstOrDefault().SupplierId.Value,
                    SalesPersonId = x.SupplierCustomers.FirstOrDefault().DefaultSalesPerson.Value,

                    City = x.Organization.Addresses.Where(x => x.Main).FirstOrDefault().City,
                    Code = x.SupplierCustomers.First().Code,
                    ZipCode = x.Organization.Addresses.Where(x => x.Main).FirstOrDefault().ZipCode,
                    Sector = x.SupplierCustomers.FirstOrDefault() !=null &&x.SupplierCustomers.FirstOrDefault().Sector !=null? x.SupplierCustomers.FirstOrDefault().Sector.Name:"",
                    SectorCode =x.SupplierCustomers.FirstOrDefault() !=null &&x.SupplierCustomers.FirstOrDefault().Sector !=null? x.SupplierCustomers.FirstOrDefault().Sector.Code  : "",
                    LimitCredit = x.SupplierCustomers.FirstOrDefault().LimitCredit,
                    Dept = x.SupplierCustomers.FirstOrDefault().Dept,
                    MonthlyObjective = x.SupplierCustomers.FirstOrDefault().MonthlyObjective,
                    CustomerState = x.SupplierCustomers.FirstOrDefault().CustomerState

                }).FirstOrDefaultAsync(x => x.OrganizationId == request.OrganizationId, cancellationToken);
            return result;
        }

        public async Task<SyncPagedResult<CustomerDto>> Handle(PagedCustomersBySalesPerson request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);
            var predicate = SharedCustomers(currentUser);

            var query = _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(predicate);
            //.DynamicWhereQuery(request.GridQuery);
            if (request.GridQuery.Where != null)
            {
                foreach (var wherePredicate in request.GridQuery.Where[0].Predicates)
                {

                    if (wherePredicate.Value == null)
                        continue;
                    if (wherePredicate.Field == "name")
                        query = query.Where($"Customer.Organization.Name.Contains(@0)", wherePredicate.Value.ToString()?.Trim());

                    else if (wherePredicate.Field == "organizationGroupCode")
                        query = query.Where($"Customer.Organization.OrganizationGroupCode.Contains(@0)",
                            wherePredicate.Value.ToString());
                    else if (wherePredicate.Field == "sector")
                        query = query.Where($"Sector.Name.Contains(@0)", wherePredicate.Value.ToString());
                    else if (wherePredicate.Field == "phoneNumber")
                        query = query.Where(p => p.Customer.Organization.PhoneNumbers.Any(x => x.Number.Contains(wherePredicate.Value.ToString())));
                    else if (wherePredicate.Field == "sectorCode")
                        query = query.Where($"Sector.Code.Contains(@0)", wherePredicate.Value.ToString());
                }

            }

            query = query.DynamicWhereQuery(request.GridQuery);
           
            var total = await query.CountAsync(cancellationToken);
            if (request.GridQuery.Sorted?.FirstOrDefault(s => s.Name == "name")?.Direction == "descending")
                query = query.OrderByDescending(c => c.Customer.Organization.Name);
            if (request.GridQuery.Sorted?.FirstOrDefault(s => s.Name == "name")?.Direction == "ascending")
                query = query.OrderBy(c => c.Customer.Organization.Name);

                var result = query
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take)
                .Select(x => new CustomerDto
                {
                    EntityId = x.Id,
                    OrganizationGroupCode = x.Customer.Organization.OrganizationGroupCode,
                    Id = (Guid)x.CustomerId,
                    CustomerGroup = x.CustomerGroup,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    Debt = x.Dept,
                    ConventionType = x.ConventionType,
                    PaymentDeadline = x.PaymentDeadline,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    SalesGroup = x.SalesGroup,
                    LimitCredit = x.LimitCredit,
                    Sector = x.Sector.Name,
                    SectorCode = x.Sector.Code,
                    CustomerState = x.CustomerState,
                    PaymentMode = x.PaymentMode,
                    PaymentMethod = x.PaymentMethod,
                    CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                    MonthlyObjective = x.MonthlyObjective,
                    AddressDto = new AddressDto(x.Customer.Organization.Addresses.FirstOrDefault(ax => ax.Main)),
                    PhoneNumber = x.Customer.Organization.PhoneNumbers.Any()
                        ? x.Customer.Organization.PhoneNumbers.FirstOrDefault().CountryCode + " " + x.Customer.Organization.PhoneNumbers.FirstOrDefault().Number
                        : ""

                });
            return new SyncPagedResult<CustomerDto>
            { Count = total, Result = (await result.ToListAsync(cancellationToken: cancellationToken)) };
        }

        public async Task<CustomerDto> Handle(GetCustomerForSalesPersonById request, CancellationToken cancellationToken)
        {
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var predicate = SharedCustomers(currentUser);
            var query = _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(predicate)
                .Where(x => x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked && x.CustomerId == request.CustomerId);
            var customer = query.Select(x => new CustomerDto
            {
                Id = (Guid)x.CustomerId,
                CustomerGroup = x.CustomerGroup,
                OrganizationId = x.Customer.OrganizationId,
                Code = x.Code,
                Name = x.Customer.Organization.Name,
                Debt = x.Dept,
                ConventionType = x.ConventionType,
                PaymentDeadline = x.PaymentDeadline,
                DefaultSalesPerson = x.DefaultSalesPerson,
                SalesGroup = x.SalesGroup,
                SalesPersonName = x.SalesPersonName,
                LimitCredit = x.LimitCredit,
                Sector =x.Sector!=null? x.Sector.Name : "",
                SectorCode =x.Sector!=null? x.Sector.Code: "",
                CustomerState = x.CustomerState,
                PaymentMode = x.PaymentMode,
                PaymentMethod = x.PaymentMethod,
                CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                MonthlyObjective = x.MonthlyObjective,
                AddressDto = new AddressDto(x.Customer.Organization.Addresses.FirstOrDefault(ax => ax.Main)),
                PhoneNumber = x.Customer.Organization.PhoneNumbers.Any()
                    ? x.Customer.Organization.PhoneNumbers.FirstOrDefault().CountryCode + " " + x.Customer.Organization.PhoneNumbers.FirstOrDefault().Number
                    : ""

            });
            return await customer.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<SyncPagedResult<CustomerDtoV2>> Handle(GetCustomerBySalesPersonDash request, CancellationToken cancellationToken)
        {
            var currentUser =
               await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                   cancellationToken);
            var salesGroup = new User();
             if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var salesPersons = await _commandBus.SendAsync(new GetSalesPersonsBySupervisorQuery { Id = currentUser.Id }, cancellationToken);
                var enumerable = salesPersons as Guid[] ?? salesPersons.ToArray();
                if (!enumerable.Any())
                    return new SyncPagedResult<CustomerDtoV2>();
                var predicateBuilder = PredicateBuilder.New<SupplierCustomer>();
                foreach (var id in enumerable)
                {
                    predicateBuilder.Or(x => x.DefaultSalesPerson == id);
                }
                var result = new List<CustomerDtoV2>();
                int count = 0;
                int take = request.DataGridQuery.Take;
                int total = 0;
                foreach (var salesPerson in salesPersons)
                {
                    if (count >= take) break;
                    take -= count;
                    //var user = await _commandBus.SendAsync(new GetUserQuery { Id = salesPerson }, cancellationToken);
                    //var predicate = SharedCustomers(salesPerson);
                    var query = _customerRepository.Table
                        .Include(x => x.Organization)
                        .Include(x => x.SupplierCustomers)
                        .Include("SupplierCustomers.Customer")
                        .Include("SupplierCustomers.Customer.Organization")
                        .Include("SupplierCustomers.Supplier")
                        .Include("SupplierCustomers.Sector")
                        .SelectMany(x => x.SupplierCustomers)
                        //.Where(predicate)
                        .Where(x => x.DefaultSalesPerson == salesPerson);
                    total += await query.CountAsync(cancellationToken);
                }
                    foreach (var salesPerson in salesPersons)
                {
                    if (count >= take) break;
                    take -= count;
                    //var user = await _commandBus.SendAsync(new GetUserQuery { Id = salesPerson }, cancellationToken);
                    //var predicate = SharedCustomers(user);
                    var query = _customerRepository.Table
                        .Include(x => x.Organization)
                        .Include(x => x.SupplierCustomers)
                        .Include("SupplierCustomers.Customer")
                        .Include("SupplierCustomers.Customer.Organization")
                        .Include("SupplierCustomers.Supplier")
                        .Include("SupplierCustomers.Sector")
                        .SelectMany(x => x.SupplierCustomers)
                        //.Where(predicate)
                        .Where(x => x.DefaultSalesPerson == salesPerson)
                        .Where(predicateBuilder)
                        .Skip(request.DataGridQuery.Skip  )
                        .Take(take);
                    count += await query.CountAsync(cancellationToken);
                    var customers = query.Select(x => new CustomerDtoV2
                    {
                        Id = (Guid)x.CustomerId,
                        Name = x.Customer.Organization.Name,
                        SalesGroup = currentUser.UserName,
                        DefaultSalesPerson = x.DefaultSalesPerson,
                        OrganizationId = x.Customer.OrganizationId,
                        CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                        MonthlyObjective = x.MonthlyObjective,
                        CustomerState = x.CustomerState,
                        DefaultSalesPersonName=  x.ActualSalesPerson.HasValue ?
                                x.ActualSalesPersonName : x.SalesPersonName
                });
                    foreach (var item in customers)
                    {
                        if (item.CustomerState != CustomerState.BlockedForOrders && item.CustomerState != CustomerState.Blocked)
                        {

                            item.HasOrderToDay = await _commandBus.SendAsync(new HasOrderToday { OrganizationId = item.OrganizationId, Date = DateTime.Now }, cancellationToken);
                            DashboardDto dashboard = new DashboardDto();
          
                            var invoice = await _commandBus.SendAsync(new GetInvoiceByCustomersQuery { Date = DateTime.Now, CustomerId = item.OrganizationId }, cancellationToken);
                            item.TotalOrdersMonthly = invoice.OrderTotalMonth;
                            item.OrderTotalMonthBenefit = invoice.OrderTotalMonthBenefit;
                            item.OrderTotalMonthBenefitRate = invoice.OrderTotalMonthBenefitRate;
                            item.OrdersPerMonth = invoice.OrdersPerMonth;
                            item.OrderTotalMonthPurchasePrice = invoice.OrderTotalMonthPurchasePrice;
                            var order = await _commandBus.SendAsync(new GetTodayOrderForCustomers { Date = DateTime.Now, CustomerId = item.OrganizationId }, cancellationToken);
                            item.CA = order.OrderTotal;
                            item.DailyMarkUpRate = order.DailyMarkUpRate;
                        }
                    } 
                    result.AddRange(await customers.ToListAsync(cancellationToken));
                } 
                return new SyncPagedResult<CustomerDtoV2> { Count = total, Result =result };
            }
             else
              if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
            {
                if (currentUser.ManagerId.HasValue)
                {
                    salesGroup = await _commandBus.SendAsync(new GetUserQuery { Id = currentUser.ManagerId.Value, IncludeRoles = true }, cancellationToken);
                }
                else { salesGroup.UserName = null; }
                var predicate = SharedCustomers(currentUser);
                var query = _customerRepository.Table
                    .Include(x => x.Organization)
                    .Include(x => x.SupplierCustomers)
                    .Include("SupplierCustomers.Customer")
                    .Include("SupplierCustomers.Customer.Organization")
                    .Include("SupplierCustomers.Supplier")
                    .Include("SupplierCustomers.Sector")
                    .SelectMany(x => x.SupplierCustomers)
                    .Distinct()
                    .Where(predicate)
                    .Where(x =>
                    x.DefaultSalesPerson == currentUser.Id);
                var total =await query.CountAsync(cancellationToken);
                query= query.Skip(request.DataGridQuery.Skip
                    )
                        .Take(request.DataGridQuery.Take); ;
                var customers =await  query.Select(x => new CustomerDtoV2
                {
                    Id = (Guid)x.CustomerId,
                    Name = x.Customer.Organization.Name,
                    SalesGroup = currentUser.UserName,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    OrganizationId = x.Customer.OrganizationId,
                    CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                    MonthlyObjective = x.MonthlyObjective,
                    CustomerState = x.CustomerState,
                    DefaultSalesPersonName=x.ActualSalesPerson.HasValue?x.ActualSalesPersonName:x.SalesPersonName                })
                    .ToListAsync(cancellationToken);
                  foreach(var item in customers)
                {
                    if (item.Name.ToLower().Contains("aniki"))
                        ;
                    if (item.CustomerState != CustomerState.BlockedForOrders && item.CustomerState != CustomerState.Blocked)
                    {
                        var defaultSalesPerson = item.DefaultSalesPerson != null ?
                        await _commandBus.SendAsync(new GetUserQuery { Id = (Guid)item.DefaultSalesPerson },
                        cancellationToken) : default;
                        item.DefaultSalesPersonName = defaultSalesPerson?.NormalizedUserName;
                        item.HasOrderToDay = await _commandBus.SendAsync(new HasOrderToday { OrganizationId = item.OrganizationId, Date = DateTime.Now }, cancellationToken);
                        DashboardDto dashboard = new DashboardDto();
                        var invoice = await _commandBus.SendAsync(new GetInvoiceByCustomersQuery { Date = DateTime.Now, CustomerId = item.OrganizationId }, cancellationToken);
                        item.TotalOrdersMonthly = invoice.OrderTotalMonth;
                        if (item.TotalOrdersMonthly > 0)
                            ;
                        item.OrderTotalMonthBenefit = invoice.OrderTotalMonthBenefit;
                        item.OrderTotalMonthBenefitRate = invoice.OrderTotalMonthBenefitRate;
                        item.OrdersPerMonth = invoice.OrdersPerMonth;
                        item.OrderTotalMonthPurchasePrice = invoice.OrderTotalMonthPurchasePrice;
                        var order = await _commandBus.SendAsync(new GetTodayOrderForCustomers { Date = DateTime.Now, CustomerId = item.OrganizationId }, cancellationToken);
                        item.CA = order.OrderTotal;
                        item.DailyMarkUpRate = order.DailyMarkUpRate;
                    }
                } 
                return new SyncPagedResult<CustomerDtoV2> { Count = total, Result =  customers };
            }
            
            return default;

        }
        public async Task<CustomerDtoV1> Handle(GetCustomerByCodeQueryV2 request, CancellationToken cancellationToken)
        {
            var organizationId = request.SupplierOrganizationId;

            var query = await _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Code == request.Code && x.Supplier.OrganizationId == organizationId )
                .Select(x => new CustomerDtoV1
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    SalesPersonId = x.DefaultSalesPerson,
                    CustomerState = x.CustomerState

                })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return query;
        }
        public async Task<CustomerDtoV1> Handle(GetCustomerByCodeQuery request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            var query = await _customerRepository.Table
                .AsNoTracking()
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Code == request.Code && x.Supplier.OrganizationId == organizationId && x.CustomerState != CustomerState.Blocked)
                .Select(x => new CustomerDtoV1
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    SalesPersonId = x.DefaultSalesPerson

                })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return query;
        }
		public async Task<IEnumerable<CustomerDtoV2>> Handle(GetCustomersBySalesPersonIdDashQuery request, CancellationToken cancellationToken)
		{
			var currentUser =
			   await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
				   cancellationToken);
			var salesGroup = new User();
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var salesPersons = await _commandBus.SendAsync(new GetSalesPersonsBySupervisorQuery { Id = currentUser.Id }, cancellationToken);
                var result = new List<CustomerDtoV2>();
                foreach (var salesPerson in salesPersons)
                {
                    var user = await _commandBus.SendAsync(new GetUserQuery { Id = salesPerson }, cancellationToken);
                    var predicate = SharedCustomers(user);
                    var query = _customerRepository.Table
                        .Include(x => x.Organization)
                        .Include(x => x.SupplierCustomers)
                        .Include("SupplierCustomers.Customer")
                        .Include("SupplierCustomers.Customer.Organization")
                        .Include("SupplierCustomers.Supplier")
                        .Include("SupplierCustomers.Sector")
                        .SelectMany(x => x.SupplierCustomers)
                        .Where(predicate)
                        .Where(x => x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked && x.DefaultSalesPerson == currentUser.Id);
                    var customers = query.Select(x => new CustomerDtoV2
                    {
                        Id = (Guid)x.CustomerId,
                        Name = x.Customer.Organization.Name,
                        SalesGroup = currentUser.UserName,
                        DefaultSalesPerson = x.DefaultSalesPerson,
                        OrganizationId = x.Customer.OrganizationId,
                        CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                        MonthlyObjective = x.MonthlyObjective 
                    }).ToList();
                    foreach (var item in customers)
                    {
                        var defaultSalesPerson = item.DefaultSalesPerson != null ?
                        await _commandBus.SendAsync(new GetUserQuery { Id = (Guid)item.DefaultSalesPerson },
                            cancellationToken) : default;
                        item.DefaultSalesPersonName = defaultSalesPerson?.NormalizedUserName;
                        item.HasOrderToDay = await _commandBus.SendAsync(new HasOrderToday { OrganizationId = item.OrganizationId, Date = DateTime.Now, SalesPersonId = currentUser.Id }
                        , cancellationToken);

                    }
                    result.AddRange(customers);
                }
                return result;
            }
          
			else
                  if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
            {
                if (currentUser.ManagerId.HasValue)
                {
                    salesGroup = await _commandBus.SendAsync(new GetUserQuery { Id = currentUser.ManagerId.Value, IncludeRoles = true }, cancellationToken);
                }
                else { salesGroup.UserName = null; }
                var predicate = SharedCustomers(currentUser);
                var query = _customerRepository.Table
                    .Include(x => x.Organization)
                    .Include(x => x.SupplierCustomers)
                    .Include("SupplierCustomers.Customer")
                    .Include("SupplierCustomers.Customer.Organization")
                    .Include("SupplierCustomers.Supplier")
                    .Include("SupplierCustomers.Sector")
                    .SelectMany(x => x.SupplierCustomers) 
                    .Where(predicate)
                    .Where(x => x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked && x.DefaultSalesPerson == currentUser.Id);
                var customers = query.Select(x => new CustomerDtoV2
                {
                    Id = (Guid)x.CustomerId,
                    Name = x.Customer.Organization.Name,
                    SalesGroup = salesGroup.UserName,
                    DefaultSalesPerson = x.DefaultSalesPerson,
                    OrganizationId = x.Customer.OrganizationId,
                    CustomerStatus = x.CustomerState != CustomerState.BlockedForOrders && x.CustomerState != CustomerState.Blocked ? "Actif" : "Bloqué",
                    MonthlyObjective = x.MonthlyObjective,

                }).ToList();
                foreach (var item in customers)
                {
                    var defaultSalesPerson = item.DefaultSalesPerson != null ?
                    await _commandBus.SendAsync(new GetUserQuery { Id = (Guid)item.DefaultSalesPerson },
                        cancellationToken) : default;
                    item.DefaultSalesPersonName = defaultSalesPerson?.NormalizedUserName;
                    item.HasOrderToDay = await _commandBus.SendAsync(new HasOrderToday { OrganizationId = item.OrganizationId, Date = DateTime.Now, SalesPersonId = currentUser.Id }
                    , cancellationToken);  
                    

                }
                return customers;
            }

            return default;
		}

        public async Task<IEnumerable<CustomerDtoV1>> Handle(GetCustomerBySalesPersonId request, CancellationToken cancellationToken)
        {
           // var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);

             var query = await _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Sector")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x =>
                           /*x.CustomerState != CustomerState.BlockedForOrders
                           && x.CustomerState != CustomerState.Blocked 
                           &&*/ (x.ActualSalesPerson==null && x.DefaultSalesPerson == request.SalesPersonId || x.ActualSalesPerson.HasValue && x.ActualSalesPerson == request.SalesPersonId))
                .Select(x => new CustomerDtoV1
                {
                    Id = (Guid)x.CustomerId,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    Sector =x.Sector!=null? x.Sector.Name  : "",
                    SectorCode = x.Sector.Code
                  
                })
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
             return query;
        }

        public async Task<SyncPagedResult<CustomerDtoV1>> Handle(GetPagedCustomersForSupervisor request, CancellationToken cancellationToken)
        {
             var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
             if (!organizationId.HasValue) return new SyncPagedResult<CustomerDtoV1>();
             var salesPersonIds = await _commandBus.SendAsync(new GetSalesPersonIdsBySalesManageQuery
                { OrganizationId = organizationId.Value, UserId = _currentUser.UserId }, cancellationToken);
             if (request.SalesPersonId.HasValue)
                 salesPersonIds = salesPersonIds.Where(x => x == request.SalesPersonId).ToList();
             var query =  _customerRepository.Table
                     .Include(x => x.Organization)
                     .Include(x => x.SupplierCustomers)
                     .Include("SupplierCustomers.Customer")
                     .Include("SupplierCustomers.Customer.Organization")
                     .Include("SupplierCustomers.Supplier")
                     .Include("SupplierCustomers.Sector")
                     .SelectMany(x => x.SupplierCustomers)
                     .Where(x =>
                         //x.CustomerState != CustomerState.BlockedForOrders
                         //&& x.CustomerState != CustomerState.Blocked &&
                         salesPersonIds.Any(s => s == x.DefaultSalesPerson.Value)
                     );
             if (request.GridQuery.Where != null)
             {
                 foreach (var wherePredicate in request.GridQuery.Where[0].Predicates)
                 {

                     if (wherePredicate.Value == null)
                         continue;
                     if (wherePredicate.Field == "name")
                     {
                         query = query.Where(
                             $"Customer.Organization.{wherePredicate.Field.UppercaseFirst()}.StartsWith(@0)",
                             wherePredicate.Value.ToString());
                         
                     }

                     else if (wherePredicate.Field == "organizationGroupCode")
                     {
                         query = query.Where($"Customer.Organization.OrganizationGroupCode.Contains(@0)",
                             wherePredicate.Value.ToString());
                         
                     }

                     else if (wherePredicate.Field == "defaultSalesPersonName")
                     {
                         wherePredicate.Field = "SalesPersonName";
                     }
                 }
             }

             query = query.DynamicWhereQuery(request.GridQuery);
           
             var total = await query.CountAsync(cancellationToken);
             if (request.GridQuery.Sorted?.FirstOrDefault(s => s.Name == "name")?.Direction == "descending")
                 query = query.OrderByDescending(c => c.Customer.Organization.Name);
             if (request.GridQuery.Sorted?.FirstOrDefault(s => s.Name == "name")?.Direction == "ascending")
                 query = query.OrderBy(c => c.Customer.Organization.Name);
             var result = query
                 .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take)
                 .Select(x => new CustomerDtoV1
                 {
                     Id = (Guid)x.CustomerId,
                     OrganizationId = x.Customer.OrganizationId,
                     Code = x.Code,
                     Name = x.Customer.Organization.Name,
                     Sector =x.Sector!=null? x.Sector.Name : "",
                     SectorCode =x.Sector!=null? x.Sector.Code  :"",
                     ActualSalesPersonName = x.ActualSalesPersonName,
                     DefaultSalesPersonName = x.SalesPersonName,
                     CustomerState = x.CustomerState

                 });
             return new SyncPagedResult<CustomerDtoV1> { Count = total, Result = _mapper.Map<List<CustomerDtoV1>>(result) };
        }

        public async Task<DashboardDto> Handle(GetDailyDashboardBySalesPersonIdDashQuery request, CancellationToken cancellationToken)
        {
            DashboardDto dashboard = new DashboardDto();

                var order = await _commandBus.SendAsync(new GetTodayOrderForCustomers {  Date = DateTime.Now }, cancellationToken);
                 

                dashboard.CA = order.OrderTotal;
                dashboard.DailyMarkUpRate = order.DailyMarkUpRate;
                return dashboard; 
        }
        public async Task<DashboardDto> Handle(GetMonthlyDashboardBySalesPersonIdDashQuery request, CancellationToken cancellationToken)
        {
            DashboardDto dashboard = new DashboardDto();


            var invoice = await _commandBus.SendAsync(new GetInvoiceByCustomersQuery { Date = DateTime.Now }, cancellationToken);

            dashboard.TotalOrdersMonthly = invoice.OrderTotalMonth;
            dashboard.OrderTotalMonthBenefit = invoice.OrderTotalMonthBenefit;
            dashboard.OrderTotalMonthBenefitRate = invoice.OrderTotalMonthBenefitRate;
            dashboard.OrdersPerMonth = invoice.OrdersPerMonth;
            dashboard.OrderTotalMonthPurchasePrice = invoice.OrderTotalMonthPurchasePrice;

            return dashboard;
        }
        public  async  Task<CustomerDtoV1> Handle(GetCustomerByCodeQueryV3 request, CancellationToken cancellationToken)
        {
           var query = await _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .Include("SupplierCustomers.Supplier.Organization")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Code == request.CustomerCode &&  x.Supplier.Organization.OrganizationGroupCode == request.OrganizationCode )
                .Select(x => new CustomerDtoV1
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                    SalesPersonId = x.DefaultSalesPerson,
                    CustomerState = x.CustomerState

                })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return query;
        }
    }
}

