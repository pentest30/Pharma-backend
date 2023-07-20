using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class QuotaCommandsHandler : 
        ICommandHandler<CreateQuotaCommand, ValidationResult>,
        ICommandHandler<ReleaseQuotaCommand, ValidationResult>,
        ICommandHandler<DecreaseQuotaCommand, ValidationResult>,
        ICommandHandler<IncreaseQuotaCommand, ValidationResult>,
        ICommandHandler<UpdateQuotaCommand, ValidationResult>,
        ICommandHandler<DecreaseQuotaAfterValidationCommand, ReserveDispatchedQuantityResponse>
    {
        private readonly IRepository<Entities.Quota, Guid> _quotaRepository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly Logger _logger;
        private readonly ICurrentUser _user;

        public QuotaCommandsHandler(IRepository<Entities.Quota, Guid> quotaRepository,
            IMapper mapper,
            ICommandBus commandBus, 
            ICurrentOrganization currentOrganization, 
            Logger logger,
            ICurrentUser user)
        {
            _quotaRepository = quotaRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _logger = logger;
            _user = user;
        }
        public async Task<ValidationResult> Handle(CreateQuotaCommand request, CancellationToken cancellationToken)
        {
         
            var validator = new CreateQuotaCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            if (!organizationId.HasValue)
            {
                validationErrors.Errors.Add(new ValidationFailure("Customer not found ", $"Your are allowed to use this resource. contact your admin"));
                return validationErrors;
            }

            var customer = default(CustomerDtoV1);
            if(request.CustomerId == Guid.Empty || request.SalesPersonId == Guid.Empty)
                customer = await _commandBus.SendAsync(new GetCustomerByCodeQuery {Code = request.CustomerCode}, cancellationToken).ConfigureAwait(false);
            else
            {
                customer = new CustomerDtoV1 { Id = request.CustomerId, Name = request.CustomerName, Code = request.CustomerCode , SalesPersonId = request.SalesPersonId};
            }

            if (customer==null)
                validationErrors.Errors.Add(new ValidationFailure("Customer not found ", $"Customer with code {request.CustomerCode} does not exist"));
            if (customer!=null&&customer.SalesPersonId==null)
                validationErrors.Errors.Add(new ValidationFailure("Customer", $"Customer with code {request.CustomerCode} does not have any sales person"));
            if (!validationErrors.IsValid)
                return validationErrors;

            var reservationValidationResult = await _commandBus.SendAsync(new ReserveDispatchedQuantityCommand{ProductId = request.ProductId, Quantity = request.InitialQuantity , IsDemand = request.IsDemand}, cancellationToken);
            if (reservationValidationResult.RemainedQuantity>0 && reservationValidationResult.ValidationResult == default)
            {
                validationErrors  = new ValidationResult();
                validationErrors.Errors.Add( new ValidationFailure("Warning", "Not all quantities were reserved. Quantity is greater than what is in the database"));
            } 
           
            else if (reservationValidationResult.ValidationResult is {IsValid: false})
                return reservationValidationResult.ValidationResult;
            var quotaProduct = await _commandBus.SendAsync(new GetQuotaProductByIdQuery {  ProductId = request.ProductId}, cancellationToken);
            if (!quotaProduct)
                return default;
            var entity = await _quotaRepository.Table
                .FirstOrDefaultAsync(x=>x.ProductId == request.ProductId
                                        && x.SalesPersonId == request.SalesPersonId
                                        && x.OrganizationId == organizationId
                                        && x.QuotaDate.Date == request.QuotaDate.ToLocalTime().Date
                    , cancellationToken: cancellationToken);
            if (entity == null && customer!=null)
            {
                if (customer.SalesPersonId.HasValue)
                {
                    entity = _mapper.Map<Entities.Quota>(request);
                    entity.QuotaDate = entity.QuotaDate.ToLocalTime().Date;
                    entity.OrganizationId = organizationId.Value;
                    entity.InitialQuantity = request.InitialQuantity /*- reservationValidationResult.RemainedQuantity*/;
                    entity.AvailableQuantity = request.InitialQuantity /*- reservationValidationResult.RemainedQuantity*/;
                    entity.SalesPersonId = customer.SalesPersonId;
                    entity.SalesPersonName = request.SalesPersonName;
                    _quotaRepository.Add(entity);
                    
                }
            }
            else if(entity!=null)
            {
                entity.InitialQuantity += request.InitialQuantity /*- reservationValidationResult.RemainedQuantity*/;
                entity.AvailableQuantity += request.InitialQuantity /*- reservationValidationResult.RemainedQuantity*/;
                _quotaRepository.Update(entity);
               
            }
            
            await _quotaRepository.UnitOfWork.SaveChangesAsync();
            return validationErrors;
        }

        
        public async Task<ValidationResult> Handle(ReleaseQuotaCommand request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            

            var entity = await _quotaRepository.Table
                .Where(x => x.ProductId == request.Id 
                            && x.QuotaDate.Date == request.Date.ToLocalTime().Date 
                            && x.OrganizationId == organizationId
                            && (request.SalesPersonId==Guid.Empty||  x.SalesPersonId == request.SalesPersonId))
                .ToListAsync(cancellationToken);
            if (entity.Count == 0)
            {
                var validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Not Found", "Entity was not found"));
                return validationErrors;
            }

            var qnt = entity.Sum(x=>x.AvailableQuantity);
            var reservationValidationResult = await _commandBus.SendAsync(new ReleaseDispatchedQuantityCommand { ProductId = request.Id, Quantity = qnt }, cancellationToken);
            if (reservationValidationResult.ValidationResult is { IsValid: false })
                return reservationValidationResult.ValidationResult;

            foreach (var quota in entity)
            {
               _quotaRepository.Delete(quota);
               var initState = new DeleteQuotaInitStateCommand();
               initState.OrganizationId = organizationId.Value;
               initState.QuotaDate = quota.QuotaDate;
               initState.ProductId = quota.ProductId;
               initState.SalesPersonId = quota.SalesPersonId.Value;
               await _commandBus.SendAsync(initState, cancellationToken);
            }
            await _quotaRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(DecreaseQuotaCommand request, CancellationToken cancellationToken)
        {
            ValidationResult validationErrors;
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            if (organizationId == default)
                throw new InvalidOperationException("User has no rights to use this resource.");
            var customer = await _commandBus
                .SendAsync(new GetCustomerByIdQuery {Id = request.CustomerId}, cancellationToken)
                .ConfigureAwait(false);
            if (customer == null)
                throw new NotFoundException($"customer with id {request.CustomerId}  was not found.");
            var quotaProduct = await _commandBus.SendAsync(new GetQuotaProductByIdQuery {  ProductId = request.ProductId}, cancellationToken);
            if (!quotaProduct)
                return default;
            var currentUser = request.SalesPersonId != Guid.Empty ? request.SalesPersonId : _user.UserId;

            var entities = await _quotaRepository.Table
                .Where(x => x.ProductId == request.ProductId
                            && x.OrganizationId == organizationId
                            && x.SalesPersonId == currentUser)
                .OrderBy(x => x.QuotaDate)
                .ToListAsync(cancellationToken);
            try
            {
                request.Quantity = FifoDecrease(request.Quantity, entities,customer);
                await UpdateQuota(request).ConfigureAwait(false);
                return null;
            }
            catch (Exception e)
            {
                _logger.Error($"-erreur  quota: {e.Message}");
                _logger.Error($"-erreur  quota: {e.InnerException?.Message}");
                validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Not Found", "Insufficient quantity"));
                return validationErrors;

            }
        }

        private int  FifoDecrease(int qnt, List<Entities.Quota> entities, CustomerDtoV1 customer, bool reserve = true)
        {
            foreach (var entity in entities)
            {
                if (qnt > entity.AvailableQuantity)
                {
                    if (entity.AvailableQuantity == 0) continue;
                    qnt -= entity.AvailableQuantity;
                    var transQnt = entity.AvailableQuantity;
                    if (reserve)
                    {
                        entity.ReservedQuantity += entity.AvailableQuantity;
                        entity.QuotaTransactions.Add(AddTransaction(entity.Id, transQnt, customer));
                    }
                    entity.AvailableQuantity = 0;
                    _quotaRepository.Update(entity);
                    continue;
                }

                entity.AvailableQuantity -= qnt;
                if (reserve)
                {
                    entity.ReservedQuantity += qnt;
                    entity.QuotaTransactions.Add(AddTransaction(entity.Id,qnt, customer));
                }
                qnt = 0;
                _quotaRepository.Update(entity);
                break;
            }

            return qnt;
        }

        private  QuotaTransaction AddTransaction(Guid id, int qnt, CustomerDtoV1 customer)
        {
            var transaction = new QuotaTransaction();
            transaction.QuotaId = id;
            transaction.Quantity = qnt;
            transaction.CustomerId = customer.Id;
            transaction.CustomerName = customer.Name;
            transaction.CustomerCode = customer.Code;
            transaction.CreatedByUserId = _user.UserId;
            transaction.CreatedDateTime = DateTimeOffset.Now;
            return transaction;
        }

        private async Task UpdateQuota(DecreaseQuotaCommand request)
        {
            await _quotaRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            _logger.Info($"-quota: Quota decreased succusfully: quantité envoyée {request.Quantity} ");
        }

        public async Task<ValidationResult> Handle(IncreaseQuotaCommand request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            if (organizationId == default)
                throw new InvalidOperationException("User has no rights to use this resource.");
            var customer = await _commandBus
                .SendAsync(new GetCustomerByIdQuery {Id = request.CustomerId}, cancellationToken)
                .ConfigureAwait(false);
            if (customer == null)
                throw new NotFoundException($"customer with id {request.CustomerId}  was not found.");
            var quotaProduct = await _commandBus.SendAsync(new GetQuotaProductByIdQuery {  ProductId = request.ProductId}, cancellationToken);
            if (!quotaProduct)
                return default;

            try
            {
                var currentUser = request.SalesPersonId != Guid.Empty ? request.SalesPersonId : _user.UserId;

                var entities = await _quotaRepository.Table
                    .Where(x => x.ProductId == request.ProductId
                                //&& x.CustomerId == customer.OrganizationId
                                && x.OrganizationId == organizationId
                                && x.SalesPersonId == currentUser)
                    .OrderByDescending(x => x.QuotaDate)
                    .ToListAsync(cancellationToken);
                request.Quantity = LifoIncrease(request, entities, customer);
                await _quotaRepository.UnitOfWork.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                _logger.Error($"-erreur  quota: {e.Message}");
                _logger.Error($"-erreur  quota: {e.InnerException?.Message}");
                var validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Not Found", "Insufficient quantity"));
                return validationErrors;
            }

            return null;
        }

        private int LifoIncrease(IncreaseQuotaCommand request, List<Entities.Quota> entities, CustomerDtoV1 customer)
        {
            foreach (var entity in entities)
            {
                if (request.Quantity> entity.ReservedQuantity)
                {
                    if(entity.ReservedQuantity == 0) continue;
                    entity.AvailableQuantity += entity.ReservedQuantity;
                    request.Quantity  -= entity.ReservedQuantity;
                    var transQnt = entity.ReservedQuantity;
                    entity.QuotaTransactions.Add(AddTransaction(entity.Id,transQnt*-1, customer));
                    entity.ReservedQuantity = 0;
                    _quotaRepository.Update(entity);
                    continue;
                }
                if(request.Quantity == 0) break;
                entity.ReservedQuantity -= request.Quantity;
                entity.AvailableQuantity += request.Quantity;
                entity.QuotaTransactions.Add(AddTransaction(entity.Id,request.Quantity*-1, customer));
                request.Quantity = 0;
                _quotaRepository.Update(entity);
                break;
            }

            return request.Quantity;
        }

        public async Task<ReserveDispatchedQuantityResponse> Handle(DecreaseQuotaAfterValidationCommand request, CancellationToken cancellationToken)
        {
            var validationErrors = default(ValidationResult);
          
            try
            {
                var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
                if (organizationId == default)
                    throw new InvalidOperationException("User has no rights to use this resource.");
                var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery(), cancellationToken);
                if (quotaProducts.All(x => x.Id != request.ProductId))
                    return default;
                var customer = await _commandBus
                    .SendAsync(new GetCustomerByIdQuery {Id = request.CustomerId}, cancellationToken)
                    .ConfigureAwait(false);
                if (customer == null)
                    throw new NotFoundException($"customer with id {request.CustomerId}  was not found.");

                var entities = await _quotaRepository.Table
                    .Where(x => x.ProductId == request.ProductId 
                                && x.SalesPersonId == request.SalesPersonId
                                && x.OrganizationId == organizationId)
                    .OrderBy(x => x.QuotaDate)
                    .ToListAsync(cancellationToken);
                request.Quantity = FifoDecrease(request.Quantity, entities, customer, false);
                await _quotaRepository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"-erreur  quota: {e.Message}");
                _logger.Error($"-erreur  quota: {e.InnerException?.Message}");
                validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Not Found", "Insufficient quantity"));
                return new ReserveDispatchedQuantityResponse {ValidationResult = validationErrors, RemainedQuantity = request.Quantity};
            }

            return  new ReserveDispatchedQuantityResponse {ValidationResult = null, RemainedQuantity = request.Quantity};
        }

        public async Task<ValidationResult> Handle(UpdateQuotaCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateQuotaCommandValidator();
            var salesPerson = await _commandBus.SendAsync(new GetUserQuery { Id = request.NewSalesPersonId.Value },
                                cancellationToken);
            var validationErrors = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationErrors.IsValid)
                return validationErrors;
            var entities = await _quotaRepository.Table
               .Where(x => x.SalesPersonId.Value == request.OldSalesPersonId.Value
               && x.ProductId == request.ProductId)
               .ToListAsync(cancellationToken);
            entities.ForEach(x => x.SalesPersonId = request.NewSalesPersonId);
            entities.ForEach(x => x.SalesPersonName = salesPerson.UserName);
            await _quotaRepository.UnitOfWork.SaveChangesAsync();
            foreach(var entity in entities)
            {
                var initState = new UpdateSalesPersonInitStateCommand();
                initState.OldSalesPersonId = request.OldSalesPersonId.Value;
                initState.OrganizationId = entity.OrganizationId;
                initState.DateTime = entity.QuotaDate;
                initState.ProductId = entity.ProductId;
                initState.SalesPersonId = request.NewSalesPersonId.Value;
                await _commandBus.SendAsync(initState, cancellationToken);
            }
            
            
            return default;
            
        }
    }
}