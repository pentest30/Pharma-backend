using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Hubs;
using GHPCommerce.Modules.Quota.Repositories;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class QuotaRequestCommandsHandler :
        ICommandHandler<CreateQuotaRequestCommand, ValidationResult>,
        ICommandHandler<ValidateQuotaRequestCommand, ValidationResult>,
        ICommandHandler<ValidateQuotaRequestCommandV2, ValidationResult>,
        ICommandHandler<ValidateQuotaRequestCommandV3>,
        ICommandHandler<RejectQuotaRequestCommand>
    {
        private readonly IQuotaRequestRepository _quotaRequestRepository;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IHubContext<QuotaNotification> _hubContext;
        private readonly ICache _redisCache;
        private readonly IQuotaRepository _quotaRepository;

        public QuotaRequestCommandsHandler(IQuotaRequestRepository quotaRequestRepository,
            ICommandBus commandBus, 
            ICurrentOrganization currentOrganization, 
            IMapper mapper, ICurrentUser currentUser, 
            IHubContext<QuotaNotification> hubContext, 
            ICache redisCache, 
            IQuotaRepository quotaRepository)
        {
            _quotaRequestRepository = quotaRequestRepository;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _currentUser = currentUser;
            _hubContext = hubContext;
            _redisCache = redisCache;
            _quotaRepository = quotaRepository;
        }
        public async Task<ValidationResult> Handle(CreateQuotaRequestCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateQuotaRequestCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            if (!validationErrors.IsValid)
                return validationErrors;
            var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = false},cancellationToken);
            var customer = await _commandBus.SendAsync(new GetCustomerByIdQuery {Id = request.CustomerId}, cancellationToken).ConfigureAwait(false);

            var entity = _mapper.Map<QuotaRequest>(request);
            entity.CustomerCode = customer.Code;
            entity.CustomerName = customer.Name;
            entity.SalesPersonId = _currentUser.UserId;
            entity.SalesPersonName = currentUser.UserName;
            if (organizationId != null) entity.OrganizationId = organizationId.Value;
            entity.Status = QuotaRequestStatus.Wait;
            _quotaRequestRepository.Add(entity);
            await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
          
            try
            {
                var connectionId = await _redisCache.GetAsync<string>(request.DestSalesPersonId.ToString(), cancellationToken);
                await _hubContext.Clients.Clients(connectionId).SendAsync("pushQuotaRequestNotification", new {userName = currentUser.UserName, message = $"Demande quota: produit {request.ProductName}, quantité {request.Quantity}"}, cancellationToken: cancellationToken).ConfigureAwait(false);

            }
            catch (Exception e)
            { 
                validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Exception", e.Message));
                return validationErrors;
            }
            return default;
        }

        public async Task<ValidationResult> Handle(ValidateQuotaRequestCommand request, CancellationToken cancellationToken)
        {
            var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
            var requestedQuota = await _quotaRequestRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

            try
            {
                if (requestedQuota == null || requestedQuota.CustomerId == default)
                    throw new NotFoundException("Entity not found or customer id has null value");
                if (requestedQuota.DestSalesPersonId.HasValue)
                {
                    var validation = await _commandBus.SendAsync(new DecreaseQuotaAfterValidationCommand
                    {
                        ProductId = requestedQuota.ProductId,
                        Quantity = requestedQuota.Quantity,
                        SalesPersonId = requestedQuota.DestSalesPersonId.Value,
                        CustomerId = requestedQuota.CustomerId.Value
                    }, cancellationToken);
                    if (validation.ValidationResult != null && !validation.ValidationResult.IsValid)
                        return validation.ValidationResult;
                    var qnt = requestedQuota.Quantity - validation.RemainedQuantity;
                    if (qnt <= 0)
                    {
                        throw new InvalidOperationException("Quantité ne peut pas etre réservée");
                    }
                    if (requestedQuota.CustomerId != null)
                    {
                        var command = await _commandBus.SendAsync(new CreateQuotaCommand
                        {
                            CustomerCode = requestedQuota.CustomerCode,
                            CustomerId = requestedQuota.CustomerId.Value,
                            CustomerName = requestedQuota.CustomerName,
                            ProductId = requestedQuota.ProductId,
                            ProductCode = requestedQuota.ProductCode,
                            ProductName = requestedQuota.ProductName,
                            QuotaDate = DateTime.Now.Date,
                            InitialQuantity = qnt,
                            SalesPersonId = requestedQuota.SalesPersonId.Value,
                            SalesPersonName = requestedQuota.SalesPersonName,
                            IsDemand = true
                        },cancellationToken);
                        
                        requestedQuota.Quantity = qnt;
                        requestedQuota.Status = QuotaRequestStatus.Validate;
                        _quotaRequestRepository.Update(requestedQuota);
                        await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
                else
                {
                    var validation = await _commandBus.SendAsync(new DecreaseQuotaAfterValidationCommand
                    {
                        ProductId = requestedQuota.ProductId,
                        Quantity = requestedQuota.Quantity,
                        SalesPersonId = requestedQuota.SalesPersonId.Value,
                        CustomerId = requestedQuota.CustomerId.Value
                    }, cancellationToken);
                    if (validation.ValidationResult != null && !validation.ValidationResult.IsValid)
                        return validation.ValidationResult;
                    var qnt = requestedQuota.Quantity - validation.RemainedQuantity;
                    if (qnt <= 0)
                    {
                        throw new InvalidOperationException("Quantité ne peut pas etre réservée");
                    }
                    if (requestedQuota.CustomerId != null)
                    {
                        var command = await _commandBus.SendAsync(new CreateQuotaCommand
                        {
                            CustomerCode = requestedQuota.CustomerCode,
                            CustomerId = requestedQuota.CustomerId.Value,
                            CustomerName = requestedQuota.CustomerName,
                            ProductId = requestedQuota.ProductId,
                            ProductCode = requestedQuota.ProductCode,
                            ProductName = requestedQuota.ProductName,
                            QuotaDate = DateTime.Now.Date,
                            InitialQuantity = qnt,
                            SalesPersonId = requestedQuota.SalesPersonId.Value,
                            SalesPersonName = requestedQuota.SalesPersonName,
                            IsDemand = true
                        }, cancellationToken);
                        requestedQuota.Quantity = qnt;
                        requestedQuota.Status = QuotaRequestStatus.Validate;
                        _quotaRequestRepository.Update(requestedQuota);
                        await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
               
               
            }
            catch (Exception e)
            {
                var validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Exception", e.Message));
                return validationErrors;
            } 

            return default;
        }

        public async Task<ValidationResult> Handle(ValidateQuotaRequestCommandV2 request, CancellationToken cancellationToken)
        {
            var requestedQuota = await _quotaRequestRepository.Table.FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken: cancellationToken);
           
            try
            {
                if (requestedQuota == null || requestedQuota.CustomerId == default)
                    throw new NotFoundException("Entity not found or customer id has null value");
                var sumOfReservedQnt = 0;
                foreach (var quotaDetail in request.QuotaDetails)
                {
                    var r = await _commandBus.SendAsync(new DecreaseQuotaAfterValidationCommand { ProductId = requestedQuota.ProductId, Quantity = quotaDetail.Quantity, SalesPersonId = requestedQuota.DestSalesPersonId.Value, CustomerId = requestedQuota.CustomerId.Value}, cancellationToken);
                    sumOfReservedQnt += r.RemainedQuantity;
                }

                var qnt = request.QuotaDetails.Sum(x=>x.Quantity) - sumOfReservedQnt;
                if (qnt <= 0)
                {
                    throw new NotFoundException("Quantité ne peut pas etre réservée");
                }
                if (requestedQuota.CustomerId != null)
                {
                    var command = await _commandBus.SendAsync(new CreateQuotaCommand
                    {
                        CustomerCode = requestedQuota.CustomerCode,
                        CustomerId = requestedQuota.CustomerId.Value,
                        CustomerName = requestedQuota.CustomerName,
                        ProductId = requestedQuota.ProductId,
                        ProductCode = requestedQuota.ProductCode,
                        ProductName = requestedQuota.ProductName,
                        QuotaDate = DateTime.Now.Date,
                        InitialQuantity = qnt,
                        SalesPersonId = requestedQuota.SalesPersonId.Value,
                        SalesPersonName = requestedQuota.SalesPersonName,

                    }, cancellationToken);
                    requestedQuota.Quantity = qnt;
                    requestedQuota.Status = QuotaRequestStatus.Validate;
                    _quotaRequestRepository.Update(requestedQuota);
                    await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var validationErrors = new ValidationResult();
                validationErrors.Errors.Add(new ValidationFailure("Not Found", e.Message));
                return validationErrors;
            }

            return default;
        }

        public async Task<Unit> Handle(ValidateQuotaRequestCommandV3 request, CancellationToken cancellationToken)
        {
            var requestedQuota = await _quotaRequestRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (requestedQuota == null) throw new NotFoundException("Entity not found...");
            requestedQuota.Status = QuotaRequestStatus.Validate;
            requestedQuota.Quantity = request.Quotas.First().InitialQuantity;
            _quotaRequestRepository.Update(requestedQuota);
            await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(RejectQuotaRequestCommand request, CancellationToken cancellationToken)
        {
            var requestedQuota = await _quotaRequestRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (requestedQuota == null) throw new NotFoundException("Entity not found...");
            requestedQuota.Status = QuotaRequestStatus.Rejected;
            _quotaRequestRepository.Update(requestedQuota);
            await _quotaRequestRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}