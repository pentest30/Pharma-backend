using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Application.Membership.Users.Commands;
using GHPCommerce.Application.Membership.Users.Queries;
using GHPCommerce.Application.Tiers.Sectors.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Commands;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class CustomersCommandsHandler :
        ICommandHandler<UpdateSupplierCustomerCommand, ValidationResult>,
        ICommandHandler<ChangeCustomerStatusCommand>,
        ICommandHandler<UpdateDebtCommand>, 
        ICommandHandler<UpdateAxCustomerCommand, ValidationResult>,
        ICommandHandler<UpdateAxCustomerDebtCommand, ValidationResult>, 
        ICommandHandler<UpdateActualSalesPersonCommand, ValidationResult>,
        ICommandHandler<RemoveActualSalesPersonCommand, ValidationResult>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;

        public CustomersCommandsHandler(IRepository<Customer, Guid> customerRepository,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,

            UserManager<User> userManager)
        {
            _customerRepository = customerRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _userManager = userManager;
            _unitOfWork = customerRepository.UnitOfWork;
            _currentUser = currentUser;

        }

        public async Task<ValidationResult> Handle(UpdateSupplierCustomerCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);
            var customer = await GetCustomer(request.OrganizationId, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"customer with {request.Id} was not found. ");
            var itemToUpdate = customer
                .SupplierCustomers
                .FirstOrDefault(x => x.Id == request.Id);
         
            if (itemToUpdate != null)
            {
                var index = customer
                    .SupplierCustomers
                    .FindIndex(x => x.Id == request.Id);
                itemToUpdate.UpdateSupplierCustomer(request.TaxGroupId, request.OnlineCustomer,
                    request.IsPickUpLocation, request.DeliveryType, request.QuotaEligibility,
                    request.DefaultSalesPerson, request.DefaultSalesGroup, request.DefaultDeliverySector,
                    request.OrganizationStatus, request.CustomerState, request.AllowedProductClasses, request.PaymentMode, request.PaymentDeadline, request.LimitCredit);
                customer.SupplierCustomers[index] = itemToUpdate;
                if (request.DefaultSalesPerson != null)
                {
                    var defaultSalesPerson = await _commandBus.SendAsync(new GetUserQuery { Id= request.DefaultSalesPerson.Value},cancellationToken);
                    customer.SupplierCustomers[index].SalesPersonName = defaultSalesPerson.UserName;
                }

                customer.UpdatedBy = currentUser.UserName;
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync();
            }

            return default;

        }

        private async Task<Customer> GetCustomer(Guid organizationId, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.Table
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.PermittedProductClasses")
                .FirstOrDefaultAsync(x => x.OrganizationId == organizationId,
                    cancellationToken);
            return customer;
        }

        public async Task<Unit> Handle(ChangeCustomerStatusCommand request, CancellationToken cancellationToken)
        {
            var customer = await GetCustomer(request.OrganizationId, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"customer with {request.Id} was not found. ");
            var itemToUpdate = customer
                .SupplierCustomers
                .FirstOrDefault(x => x.Id == request.Id);
            if (itemToUpdate != null)
            {
                var index = customer
                    .SupplierCustomers
                    .FindIndex(x => x.Id == request.Id);
                itemToUpdate.OrganizationStatus = itemToUpdate.OrganizationStatus == OrganizationStatus.Active ? OrganizationStatus.Blocked : OrganizationStatus.Active;
                customer.SupplierCustomers[index] = itemToUpdate;
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync();

            }

            return default;
        }

        public async Task<Unit> Handle(UpdateDebtCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.Table
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Supplier")
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken);

            if (customer == null)
                throw new NotFoundException($"customer with {request.Id} was not found. ");
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            var itemToUpdate = customer
                .SupplierCustomers
                .FindIndex(x => x.Supplier.OrganizationId == orgId);
            customer.SupplierCustomers[itemToUpdate].Dept += request.CurrentDebt;
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<ValidationResult> Handle(UpdateAxCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.Table
                .Include(x => x.SupplierCustomers)
                .FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken);

            if (customer == null)
                throw new NotFoundException($"customer with {request.Code} was not found. ");
            var customerSupplier = customer.SupplierCustomers.FirstOrDefault();
            if (customerSupplier != null)
            {
                if (request.DefaultSalesPerson != customerSupplier.SalesPersonName)
                    customerSupplier.DefaultSalesPerson = await GetDefaultSalesPerson(request, cancellationToken);
                customerSupplier.Dept = request.Dept;
                customerSupplier.LimitCredit = request.LimitCredit;
                customerSupplier.PaymentDeadline = request.PaymentDeadline;
                customerSupplier.PaymentMethod = request.PaymentMethod;
                customerSupplier.CustomerState = request.CustomerState;
                customerSupplier.MonthlyObjective = request.MonthlyObjective;
                customerSupplier.SalesGroup = request.DefaultSalesGroup;
                customerSupplier.CustomerGroup = request.CustomerGroup;
                var defaultSector = await _commandBus.SendAsync(new GetSectorIdByCodeQuery { Code = request.DefaultDeliverySector }, cancellationToken);
                if (defaultSector != Guid.Empty)
                    customerSupplier.DefaultDeliverySector = defaultSector;

            }

            var index = customer.SupplierCustomers.IndexOf(customerSupplier);
            customer.SupplierCustomers[index] = customerSupplier;
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        private async Task<Guid> GetDefaultSalesPerson(UpdateAxCustomerCommand request, CancellationToken cancellationToken)
        {
            var defaultSalesPerson =
                await _commandBus.SendAsync(new GetUserIdByNameQuery {UserName = request.DefaultSalesPerson},
                    cancellationToken);
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                throw new InvalidOperationException("You cannot use this resource ");

            if (defaultSalesPerson == Guid.Empty)
            {
                var user = new User
                {
                    UserName = request.DefaultSalesPerson,
                    NormalizedUserName = request.DefaultSalesPerson.ToUpper(),
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

            return defaultSalesPerson;
        }

        public async Task<ValidationResult> Handle(UpdateAxCustomerDebtCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.Table
                .Include(x => x.SupplierCustomers)
                .FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken);

            if (customer == null)
                throw new NotFoundException($"customer with {request.Code} was not found. ");
            var customerSupplier = customer.SupplierCustomers.FirstOrDefault();
            if (customerSupplier != null)
            {
                customerSupplier.Dept = request.Debt;
                var index = customer.SupplierCustomers.IndexOf(customerSupplier);
                customer.SupplierCustomers[index] = customerSupplier;
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync();
              
            }
            return default;
        }

        public  async Task<ValidationResult> Handle(UpdateActualSalesPersonCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            foreach (var requestCustomer in request.Customers)
            {
                var customer = await _customerRepository.Table
                    .Include(x => x.SupplierCustomers)
                    .Include("SupplierCustomers.Supplier")
                    .FirstOrDefaultAsync(x => x.Id == requestCustomer,cancellationToken);
                var itemToUpdate = customer
                    .SupplierCustomers
                    .FindIndex(x => x.Supplier.OrganizationId == orgId 
                                    && x.DefaultSalesPerson == request.SalespersonsSourceId);
                if(itemToUpdate<0) continue;
                customer.SupplierCustomers[itemToUpdate].ActualSalesPerson = request.SalespersonsDestinationId;
                customer.SupplierCustomers[itemToUpdate].ActualSalesPersonName = request.SalespersonsDestination;
                _customerRepository.Update(customer);
            }
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public  async  Task<ValidationResult> Handle(RemoveActualSalesPersonCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            foreach (var requestSalesPersonId in request.SalesPersonIds)
            {
                var query = await (from q in _customerRepository.Table.Include(x=>x.SupplierCustomers)
                    from a in q.SupplierCustomers
                    where a.DefaultSalesPerson == requestSalesPersonId && a.Supplier.OrganizationId == orgId
                    select q)
                    .ToListAsync(cancellationToken: cancellationToken);
                if (query .Any())
                {
                    foreach (var customer in query)
                    {
                        var itemToUpdate = customer
                            .SupplierCustomers
                            .FindIndex(x =>x.DefaultSalesPerson == requestSalesPersonId && x.ActualSalesPerson.HasValue);
                        if(itemToUpdate<0) continue;
                        customer.SupplierCustomers[itemToUpdate].ActualSalesPerson = null;
                        customer.SupplierCustomers[itemToUpdate].ActualSalesPersonName = null;
                        _customerRepository.Update(customer);
                    }
                }
               
                
            }
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
