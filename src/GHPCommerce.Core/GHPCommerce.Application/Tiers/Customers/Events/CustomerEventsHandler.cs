using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.ProductClasses.Queries;
using GHPCommerce.Application.Membership.Users.Commands;
using GHPCommerce.Application.Membership.Users.Queries;
using GHPCommerce.Application.Tiers.Organizations.Events;
using GHPCommerce.Application.Tiers.Sectors.Queries;
using GHPCommerce.Application.Tiers.Suppliers.Events;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Events;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Customers.Events
{
    public class CustomerEventsHandler :
        IEventHandler<SupplierCreatedEvent>,
        IEventHandler<OrganizationCustomerCreatedEvent>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public CustomerEventsHandler(IRepository<Customer, Guid> customerRepository, 
            IRepository<Supplier, Guid> supplierRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, 
            ICommandBus commandBus,
            UserManager<User> userManager)
        {
            _customerRepository = customerRepository;
            _supplierRepository = supplierRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
            _userManager = userManager;
            _unitOfWork = _customerRepository.UnitOfWork;
        }

        public async Task Handle(SupplierCreatedEvent notification, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (notification.IsSupplier&&  orgId == null)
                throw new InvalidOperationException("Resources not allowed");
            var currentOrgId = orgId.Value;
            var customerCode = 0;
            var incrementalCode = "";
            if (!notification.IsSupplier) currentOrgId = notification.OrganizationId;
            var lastCode = (await _customerRepository.Table
                .Include(x=> x.SupplierCustomers)
                .SelectMany(x=>x.SupplierCustomers)
                .OrderByDescending(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken))?.Code;
            if (string.IsNullOrEmpty(lastCode)) customerCode++;
            else
            {
                var resultString = Regex.Match(lastCode, @"\d+").Value;
                customerCode = int.Parse(resultString) +1;

            }
            var customer = await _customerRepository.Table
                .Where(x => x.OrganizationId == currentOrgId)
                .Include(x => x.SupplierCustomers)
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(cancellationToken);
            if (customer == null)
            {
                customer = new Customer {OrganizationId = currentOrgId};
                incrementalCode="H" + customerCode.ToString().PadLeft(5, '0');
                await SaveCustomer(customer, notification, incrementalCode);
            }
            else
            {

                // ReSharper disable once ComplexConditionExpression
                if (customer.SupplierCustomers != null &&
                    customer.SupplierCustomers.All(x => x.SupplierId != notification.SupplierId))
                    await SaveCustomer(customer, notification, string.Empty);
            }
        }

        private async Task SaveCustomer(Customer customer, SupplierCreatedEvent @event, string code)
        {
            var supplierCustomer = InitSupplierCustomer(@event, code);
            customer.SupplierCustomers.Add(supplierCustomer);
            if (customer.Id == Guid.Empty)  _customerRepository.Add(customer);
            else _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        private SupplierCustomer InitSupplierCustomer(SupplierCreatedEvent notification, string code)
        {
            SupplierCustomer supplierCustomer = _mapper.Map<SupplierCustomer>(notification);
            supplierCustomer.PermittedProductClasses = new List<AllowedProductClass>();

            foreach (var item in notification.AllowedProductClasses)
            {
                supplierCustomer.PermittedProductClasses.Add(new AllowedProductClass
                {
                    ProductClassId = item
                });
            }

            supplierCustomer.SupplierId = notification.SupplierId;
            supplierCustomer.Code = code;
            return supplierCustomer;
        }

        public async Task Handle(OrganizationCustomerCreatedEvent notification, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                throw new InvalidOperationException("You cannot use this resource ");
            var customer = await _customerRepository.Table
                .Include(x=>x.SupplierCustomers)
                .FirstOrDefaultAsync(x => x.OrganizationId == notification.OrganizationId, cancellationToken: cancellationToken);
            SupplierCustomer supplierCustomer = _mapper.Map<SupplierCustomer>(notification);
            var classes = await _commandBus.SendAsync(new GetAllProductClassesQuery(), cancellationToken);
            supplierCustomer.PermittedProductClasses  = new List<AllowedProductClass>();
            foreach (var item in  classes)
            {
                supplierCustomer.PermittedProductClasses.Add(new AllowedProductClass
                {
                    ProductClassId = item.Id
                });
            }
            var defaultSalesPerson = await _commandBus.SendAsync(new GetUserIdByNameQuery {UserName = notification.DefaultSalesPerson}, cancellationToken);
            if (defaultSalesPerson == Guid.Empty)
            {
                var email = notification.DefaultSalesPerson + "@groupehydrapharm.com";
                var user = new User
                {
                    UserName = notification.DefaultSalesPerson,
                    NormalizedUserName = notification.DefaultSalesPerson.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = null,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    OrganizationId = orgId
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _commandBus.Send(new AddUserRoleCommand
                    {
                        Id = user.Id,
                        Role = new UserRole
                        {
                            RoleId = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0210b")
                        },
                    }, cancellationToken);
                    defaultSalesPerson = user.Id;
                }

            }

            supplierCustomer.DefaultSalesPerson = defaultSalesPerson;
            supplierCustomer.SalesPersonName = notification.DefaultSalesPerson;
            var supplierId =await _supplierRepository.Table
                .Include(x=>x.Organization)
                .Where(x => x.OrganizationId ==  orgId)
                .Select(x=>x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (supplierId == Guid.Empty)
            {
                var supplier = new Supplier { OrganizationId = orgId.Value };
                _supplierRepository.Add( supplier );
                await _supplierRepository.UnitOfWork.SaveChangesAsync();
                supplierId = supplier.Id;
            }
            supplierCustomer.SupplierId = supplierId;
            supplierCustomer.SalesGroup = notification.DefaultSalesGroup;
            supplierCustomer.Code = notification.Code;
            var defaultSector = await _commandBus.SendAsync(new GetSectorIdByCodeQuery {Code = notification.DefaultDeliverySector}, cancellationToken);
            if (defaultSector != Guid.Empty)
                supplierCustomer.DefaultDeliverySector = defaultSector;
            if (customer == null)
            {
                customer = new Customer {OrganizationId = notification.OrganizationId, Code = notification.Code};
                customer.SupplierCustomers.Add(supplierCustomer);
                _customerRepository.Add(customer);
            }
            else
            {
                supplierCustomer.CustomerId = customer.Id;
                if (customer.SupplierCustomers.Any())
                {
                    customer.SupplierCustomers[0].DefaultSalesPerson = supplierCustomer.DefaultSalesPerson;
                    customer.SupplierCustomers[0].CustomerGroup = supplierCustomer.CustomerGroup;
                    customer.SupplierCustomers[0].SalesGroup = supplierCustomer.SalesGroup;
                    customer.SupplierCustomers[0].SalesPersonName = supplierCustomer.SalesPersonName;
                    customer.SupplierCustomers[0].DefaultSalesGroup = supplierCustomer.DefaultSalesGroup;
                    customer.SupplierCustomers[0].DefaultDeliverySector = supplierCustomer.DefaultDeliverySector;
                    //customer.SupplierCustomers[0].PermittedProductClasses = supplierCustomer.PermittedProductClasses;
                    customer.SupplierCustomers[0].LimitCredit = supplierCustomer.LimitCredit;
                    customer.SupplierCustomers[0].Dept = supplierCustomer.Dept;
                    customer.SupplierCustomers[0].PaymentDeadline = supplierCustomer.PaymentDeadline; 
                    customer.SupplierCustomers[0].PaymentMethod = supplierCustomer.PaymentMethod;
                    customer.SupplierCustomers[0].MonthlyObjective = supplierCustomer.MonthlyObjective;
                    customer.SupplierCustomers[0].CustomerState = supplierCustomer.CustomerState;
                    customer.SupplierCustomers[0].ConventionType = supplierCustomer.ConventionType;
                    customer.SupplierCustomers[0].PaymentMode = supplierCustomer.PaymentMode;
                }
                else customer.SupplierCustomers.Add(supplierCustomer);
                _customerRepository.Update(customer);
            }
           
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
