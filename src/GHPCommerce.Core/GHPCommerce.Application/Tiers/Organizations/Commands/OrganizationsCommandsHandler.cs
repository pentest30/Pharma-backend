using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Application.Tiers.Organizations.Events;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

namespace GHPCommerce.Application.Tiers.Organizations.Commands
{
 
    public class OrganizationsCommandsHandler :
        ICommandHandler<CreateOrganizationCommand, ValidationResult>,
        ICommandHandler<UpdateOrganizationCommand, ValidationResult>,
        ICommandHandler<DeleteOrganizationCommand>,
        ICommandHandler<ActivateOrganizationCommand>,
        ICommandHandler<CreateAXOrganizationCommand, ValidationResult>

    {
        private readonly IRepository<Organization, Guid> _orgRepository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly Logger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;

        public OrganizationsCommandsHandler(IRepository<Organization, Guid> orgRepository,
            IMapper mapper,
            ICurrentUser currentUser,
            ICommandBus commandBus, Logger logger)
        {
            _orgRepository = orgRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _logger = logger;
            _unitOfWork = _orgRepository.UnitOfWork;
            _currentUser = currentUser;

        }
        public async Task<ValidationResult> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);

            var existingOrg = await _orgRepository.Table.AnyAsync(x => x.OrganizationGroupCode == request.Code, cancellationToken: cancellationToken);
            if (existingOrg)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Code", "Une entité avec le mème code existe dans la base de données !")
                    }
                };
            }
            var organization = _mapper.Map<Organization>(request);
            organization.CreatedBy = currentUser.UserName;
            _orgRepository.Add(organization);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);

            var existingOrg = await _orgRepository.Table
                .AsTracking()
                .Include(x => x.Addresses)
                .Include(x => x.BankAccounts)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.Emails)
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (existingOrg == null)
                throw new NotFoundException($"Organization with {request.Id} wasn't found");

            existingOrg.UpdateOrganization(request.Name, request.NIS, request.NIF, request.RC, request.AI,
                request.DisabledReason, request.Owner, request.OrganizationStatus, request.Activity,
                request.EstablishmentDate, request.Addresses, request.PhoneNumbers, request.BankAccounts,
                request.UserAccounts, request.Emails);
            existingOrg.UpdatedBy = currentUser.UserName;
            existingOrg.OrganizationGroupCode = request.Code;
            _orgRepository.AddOrUpdate(existingOrg);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
        {
            var existingOrg = await _orgRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (existingOrg == null)
                throw new NotFoundException($"Organization with {request.Id} wasn't found");
            existingOrg.OrganizationStatus = OrganizationStatus.Blocked;
            _orgRepository.Update(existingOrg);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<Unit> Handle(ActivateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var existingOrg = await _orgRepository.Table
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (existingOrg == null)
                throw new NotFoundException($"Organization with {request.Id} wasn't found");
            existingOrg.OrganizationStatus = OrganizationStatus.Active;
            _orgRepository.Update(existingOrg);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(CreateAXOrganizationCommand request, CancellationToken cancellationToken)
        {
            await LockProvider<string>.WaitAsync(request.Code, cancellationToken);
            var validator = new CreateAXOrganizationCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
            {
                foreach (var validationErrorsError in validationErrors.Errors)
                {
                    _logger.Error(validationErrorsError?.ErrorMessage);
                }
                LockProvider<string>.Release(request.Code);
                return validationErrors;
            }

            try
            {

                var organization = await(from or in _orgRepository
                            .Table
                            .Include(x => x.Addresses)
                            .Include(x => x.Customer)
                            .Include(x => x.Customer.SupplierCustomers)
                        from cls in or.Customer.SupplierCustomers
                        where cls.Code == request.Code
                        select or)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
               
            
                if (organization == null)
                {
                    organization = _mapper.Map<Organization>(request);
                    organization.OrganizationActivity = request.OrganizationActivity;
                    organization.Activity = OrganizationActivity.Pharmacist;
                    organization.OrganizationGroupCode = request.Code; //Par défaut officine
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        organization.Addresses.Add(new Address
                        {
                            City = request.City,
                            Country = request.Country,
                            State = request.State,
                            Street = request.Street,
                            Main = true,
                            ZipCode = request.ZipCode,
                            Billing = true,
                            Shipping = true
                        });
                    }
                    _orgRepository.Add(organization);
              
                }
                else
                {
                    organization.Name = request.Name;
                    organization.NIF = request.NIF;
                    organization.NIS = request.NIS;
                    organization.AI = request.AI;
                    organization.RC = request.RC;
                    organization.OrganizationActivity = request.OrganizationActivity;
                    //organization.OrganizationGroupCode = request.OrganizationGroupCode;
                    var address = new Address
                    {
                        City = request.City,
                        Country = request.Country,
                        State = request.State,
                        Street = request.Street,
                        Main = true,
                        Billing = true,
                        Shipping = true,
                        ZipCode = request.ZipCode
                    };
                    if (!organization.Addresses.Any() && !string.IsNullOrEmpty(address.City))
                    {
                        organization.Addresses.Add(address);
                    }
                    else if(!string.IsNullOrEmpty(address.City))
                    {
                        organization.Addresses[0].City = request.City;
                        organization.Addresses[0].State = request.State;
                        organization.Addresses[0].Street = request.Street;
                        organization.Addresses[0].ZipCode = request.ZipCode;
                    }
                    _orgRepository.Update(organization);
                }

                await _orgRepository.UnitOfWork.SaveChangesAsync();
                var customerCreated = new OrganizationCustomerCreatedEvent
                {
                    Code = request.Code,
                    CustomerGroup = request.CustomerGroup,
                    CustomerState = request.CustomerState,
                    DefaultDeliverySector = request.DefaultDeliverySector,
                    DefaultSalesPerson = request.DefaultSalesPerson,
                    Dept = request.Dept,
                    LimitCredit = request.LimitCredit,
                    OrganizationId = organization.Id,
                    MonthlyObjective = request.MonthlyObjective,
                    PaymentDeadline = request.PaymentDeadline,
                    PaymentMethod = request.PaymentMethod

                };
                await _commandBus.Publish(customerCreated, cancellationToken);
                LockProvider<string>.Release(request.Code);
                return default;
            }
            catch (Exception e)
            {
                LockProvider<string>.Release( request.Code);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                return new ValidationResult
                    { Errors = { new ValidationFailure("Produit", e.Message) } };

            }
        }
    }
}
