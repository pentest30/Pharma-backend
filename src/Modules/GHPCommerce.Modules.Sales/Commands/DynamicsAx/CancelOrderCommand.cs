using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.PreparationOrder.Commands;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;
using ServiceReference1;

namespace GHPCommerce.Modules.Sales.Commands.DynamicsAx
{
    public class CancelOrderCommand :ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }

    public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, ValidationResult>
    {
        private readonly IRepository<Order, Guid> _repository;
        private readonly ICache _cache;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly Logger _logger;
        private readonly MedIJKModel _model;

        public CancelOrderCommandHandler(IRepository<Order, Guid> repository,
            ICache cache,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            Logger logger, MedIJKModel model)
        {
            _repository = repository;
            _cache = cache;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _logger = logger;
            _model = model;
        }

        public async Task<ValidationResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (!orgId.HasValue)
                    throw new InvalidOperationException("Resources not allowed");

                var order = await _repository.Table
                    .Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == request.Id && !o.QuantitiesReleased, cancellationToken);
                if (order == null)
                    throw new NotFoundException($"Order with number {request.Id} was not found");
                var isConsolidated = await _commandBus.SendAsync(new GetConsolidateOrderByIdQuery { OrderId = order.Id }, cancellationToken);
                if (isConsolidated)
                    throw new InvalidOperationException($"Vous ne pouvez pas annuler une commande consolidée/facturée");
                var preparationOrderItems = await _commandBus.SendAsync(new GetPOsByOrderQuery { OrderId = order.Id }, cancellationToken);
                var customer = await _commandBus.SendAsync(
                    new GetCustomerByOrganizationIdQuery { OrganizationId = order.CustomerId }, cancellationToken);

                foreach (var item in order.OrderItems)
                {
                    string key = item.ProductId.ToString() + orgId.Value;
                    await LockProvider<string>.ProvideLockObject(key).WaitAsync(cancellationToken);
                    var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                    var index = inventSum
                        .CachedInventSumCollection
                        .CachedInventSums
                        .FindIndex(x =>x.ProductId == item.ProductId && x.InternalBatchNumber == item.InternalBatchNumber);
                    if (index == -1)
                    {
                        LockProvider<string>.ProvideLockObject(key).Release();
                        continue;
                    }
                    var qnt = GetRestQuantity(preparationOrderItems, item);
                    if (qnt.Item1 == 0)
                    {
                        await IncreaseQuota(customer.Id, order.CreatedByUserId, item.ProductId, qnt.Item2, cancellationToken);
                        LockProvider<string>.ProvideLockObject(key).Release();
                        continue;
                    }

                    if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >= qnt.Item1)
                        inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -= qnt.Item1;
                    else
                        inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;
                    await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                    await _commandBus.SendAsync(
                        new ReleaseReservedQuantityCommandV2
                        {
                            ProductId = item.ProductId,
                            Quantity = qnt.Item1,
                            InternalBatchNumber = item.InternalBatchNumber
                        }, cancellationToken);
                   
                    await IncreaseQuota(customer.Id, order.CreatedByUserId, item.ProductId, qnt.Item2, cancellationToken);
                    LockProvider<string>.ProvideLockObject(key).Release();
                }
                order.QuantitiesReleased = true;
                order.OrderStatus = OrderStatus.Canceled;
                await _commandBus.SendAsync(new CancelPreparationsForOrderCommand { OrderId = order.Id }, cancellationToken);
                _repository.Update(order);

                await _repository.UnitOfWork.SaveChangesAsync();
                #region Cancel Order in AX if already created
                if (!string.IsNullOrEmpty(order.CodeAx))
                    if (!await CancelOrderAX(order.CodeAx, cancellationToken))
                        throw new InvalidOperationException($"La commande n'a pas pu être annulée sur AX ");

                #endregion

            }

            catch (Exception ex)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }

            return default;
        }

        private Tuple<int, int> GetRestQuantity(List<PreparationOrderItemDtoV1> items, OrderItem orderItem)
        {
            var quantity = orderItem.Quantity;
            var sumOfQuantitiesControlled = items
                .Where(x => x.ProductId == orderItem.ProductId && x.IsControlled &&  x.OldQuantity.HasValue && x.OldQuantity>0)
                .Sum(x=>x.OldQuantity.Value);
            var sumOfQuantities = items
               .Where(x => x.ProductId == orderItem.ProductId &&  x.Status!=Core.Shared.Events.PreparationOrder.BlStatus.Deleted)
               .Sum(x => x.Quantity);
            if (sumOfQuantitiesControlled == 0) return  new Tuple<int, int> (quantity, quantity);
            return quantity > sumOfQuantitiesControlled ? new Tuple<int, int>(quantity - sumOfQuantitiesControlled, sumOfQuantities) : new Tuple<int, int>(0, sumOfQuantities);
        }

        private async Task IncreaseQuota(Guid customerId, Guid orderCreatedByUserId, Guid productId, int qnt, CancellationToken cancellationToken)
        {
            await _commandBus.SendAsync(new IncreaseQuotaCommand
            {
                ProductId = productId,
                CustomerId = customerId,
                SalesPersonId = orderCreatedByUserId,
                Quantity = qnt
            }, cancellationToken);
        }

        private async Task<bool> CancelOrderAX(string codeAx, CancellationToken cancellationToken)
        {           
            if (!string.IsNullOrEmpty(codeAx))
            {
                DOSI_SalesOrderServiceClient client = new DOSI_SalesOrderServiceClient();
                CallContext callContext = new CallContext();
                callContext.Company = "HP";
                client.ClientCredentials.Windows.ClientCredential.UserName = _model.UserAx;
                client.ClientCredentials.Windows.ClientCredential.Password = _model.PasswordAx; // Code société dans AX
                var msg = await client.cancelSalesOrderAsync(callContext, codeAx);
                // Voir les messages d'erreur
                if (msg.response!.comments != null)
                {
                    var errorMsg = "";
                    foreach (KeyValuePair<int, String> m in msg.response.comments)
                    {
                        errorMsg += m.Value + "\r";
                    }
                    _logger.Error($"Erreur lors de l'annulation de la commande AX {codeAx} : {errorMsg}");
                    throw new InvalidOperationException(errorMsg);
                }

                return true;
            }

            return false;
        }

    }
}