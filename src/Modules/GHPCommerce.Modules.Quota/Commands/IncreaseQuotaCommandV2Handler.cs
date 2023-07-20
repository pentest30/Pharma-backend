using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class IncreaseQuotaCommandV2Handler : ICommandHandler<IncreaseQuotaCommandV2, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<QuotaDbContext> _factory;
        //private readonly Repository<Entities.Quota, Guid> _quotaRepository;
        private readonly Logger _logger;

        public IncreaseQuotaCommandV2Handler(ICommandBus commandBus, Func<QuotaDbContext> factory,
            Logger logger)
        {
            _commandBus = commandBus;
            _factory = factory;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(IncreaseQuotaCommandV2 request, CancellationToken cancellationToken)
        {
            var customer = await _commandBus
                .SendAsync(new GetCustomerByIdQuery {Id = request.CustomerId}, cancellationToken)
                .ConfigureAwait(false);
          
          

            try
            {
                if (customer == null)
                    throw new NotFoundException($"customer with id {request.CustomerId}  was not found.");
                var quotaProduct = await _commandBus.SendAsync(new GetQuotaProductByIdQuery {  ProductId = request.ProductId}, cancellationToken);
                if (!quotaProduct)
                    return default;
                var currentUser = request.SalesPersonId;
                using (var ctx = _factory.Invoke())
                {
                    var entities = await ctx.Set<Entities.Quota>()
                        .Where(x => x.ProductId == request.ProductId
                                    //&& x.CustomerId == customer.OrganizationId
                                    && x.OrganizationId == request.OrganizationId
                                    && x.SalesPersonId == currentUser)
                        .OrderByDescending(x => x.QuotaDate)
                        .ToListAsync(cancellationToken);
                    request.Quantity = LifoIncrease(request, entities, customer);
                    await ctx.SaveChangesAsync();
                }
              
                
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
        private  QuotaTransaction AddTransaction(Guid id, int qnt,Guid userId,  CustomerDtoV1 customer)
        {
            var transaction = new QuotaTransaction();
            transaction.QuotaId = id;
            transaction.Quantity = qnt;
            transaction.CustomerId = customer.Id;
            transaction.CustomerName = customer.Name;
            transaction.CustomerCode = customer.Code;
            transaction.CreatedByUserId =userId;
            transaction.CreatedDateTime = DateTimeOffset.Now;
            return transaction;
        }
        private int LifoIncrease(IncreaseQuotaCommandV2 request, List<Entities.Quota> entities, CustomerDtoV1 customer)
        {
            foreach (var entity in entities)
            {
                if (request.Quantity> entity.ReservedQuantity)
                {
                    if(entity.ReservedQuantity == 0) continue;
                    entity.AvailableQuantity += entity.ReservedQuantity;
                    request.Quantity  -= entity.ReservedQuantity;
                    var transQnt = entity.ReservedQuantity;
                    entity.QuotaTransactions.Add(AddTransaction(entity.Id,transQnt*-1,request.SalesPersonId ,customer));
                    entity.ReservedQuantity = 0;
                    //_quotaRepository.Update(entity);
                    continue;
                }
                if(request.Quantity == 0) break;
                entity.ReservedQuantity -= request.Quantity;
                entity.AvailableQuantity += request.Quantity;
                entity.QuotaTransactions.Add(AddTransaction(entity.Id,request.Quantity*-1,request.SalesPersonId, customer));
                request.Quantity = 0;
               // _quotaRepository.Update(entity);
                break;
            }

            return request.Quantity;
        }

       
    }
}