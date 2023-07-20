using GHPCommerce.Domain.Domain.Commands;
using System;
using GHPCommerce.Modules.PreparationOrder.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Services.ExternalServices;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;
using PreparationOrderStatus = GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderStatus;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Orders;


namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class MakeOrderAsToBeShippedCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }
    public class UpdatePreparationOrderCommand : ICommand<AxValidationDto>
    {
        public Guid Id { get; set; }
        public Guid? ConsolidatedById { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public string ConsolidatedByName { get; set; }
        public string EmployeeCode { get; set; }
        public string receivedByCode { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public PreparationOrderStatus PreparationOrderStatus { get; set; }

        public List<PreparationOrderItem> PreparationOrderItems { get; set; }
        public Guid PickingZoneId { get; set; }
        public String PickingZoneName { get; set; }
        public Guid ExecutedById { get; set; }
        public String ExecutedByName { get; set; }
        public Guid VerifiedById { get; set; }

        public String VerifiedByName { get; set; }
    }

    public class UpdatePreparationOrderCommandHandler : ICommandHandler<UpdatePreparationOrderCommand, AxValidationDto>,
        ICommandHandler<MakeOrderAsToBeShippedCommand, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        private readonly IPreparationOrderRepository _preparationOrderRepository;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly MedIJKModel _model;

        public UpdatePreparationOrderCommandHandler(ICommandBus commandBus,
            IPreparationOrderRepository preparationOrderRepository,
            ICache redisCache,
            Logger logger,
            ICurrentOrganization currentOrganization,
            IMapper mapper, MedIJKModel model)
        {
            _commandBus = commandBus;
            _preparationOrderRepository = preparationOrderRepository;
            _redisCache = redisCache;
            _logger = logger;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _model = model;
        }

        public async Task<AxValidationDto> Handle(UpdatePreparationOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await LockProvider<string>.ProvideLockObject(request.Id.ToString()).WaitAsync(cancellationToken);
                if (request.PickingZoneId == Guid.Empty || request.VerifiedById == Guid.Empty || request.ExecutedById == Guid.Empty)
                {
                    var error = new Dictionary<string, string>();
                    error.Add("Erreur de validation", "l'agent de prépation est obligatoire");
                    return new AxValidationDto { OrderValidationErrors = error };
                }
                var bl = await _preparationOrderRepository.Table
                    .AsTracking()
                    .Include(c => c.PreparationOrderItems)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = bl.OrderId }, cancellationToken);
                if (order.OrderStatus == 70 || order.OrderStatus == 200)
                {
                    var error = new Dictionary<string, string>();
                    error.Add("Erreur de validation", "Cette commande a été annulée par le service commercial. ");
                    return new AxValidationDto { OrderValidationErrors = error };
                }

                bl.TotalPackage = request.TotalPackage;
                bl.TotalPackageThermolabile = request.TotalPackageThermolabile != 0 ? request.TotalPackageThermolabile : bl.TotalPackageThermolabile;
                bl.EmployeeCode = request.EmployeeCode;
                var newItems = request.PreparationOrderItems.Where(c => c.Status == BlStatus.New).ToList();
                if (newItems.Any())
                {
                    newItems.ForEach(it =>
                    {
                        //un check pour éviter la duplication des lignes op 
                        if (bl.PreparationOrderItems.Any(x =>
                                x.ProductId == it.ProductId && x.InternalBatchNumber == it.InternalBatchNumber))
                        {
                            return;
                        }
                        it.Id = Guid.Empty;
                        bl.PreparationOrderItems.Add(it);
                    });
                }

              
                foreach (var item in bl.PreparationOrderItems)
                {
                    // en cas le lot est changé
                    var preparationOrderItem = request.PreparationOrderItems
                        .FirstOrDefault(c => c.ProductId == item.ProductId && c.PreviousInternalBatchNumber == item.InternalBatchNumber
                        && item.OldQuantity.HasValue
                        && item.OldQuantity > 0);

                    if (preparationOrderItem != null && preparationOrderItem.InternalBatchNumber != preparationOrderItem.PreviousInternalBatchNumber)
                    {

                        item.IsControlled = true;
                        item.Quantity = preparationOrderItem.Quantity;
                        item.Packing = preparationOrderItem.Packing;
                        item.Status = preparationOrderItem.Status;
                        //To be reviewed by iqbal , this ensures that the previous contains always the first assigned value(ordered batch)
                        item.PreviousInternalBatchNumber = string.IsNullOrEmpty(item.PreviousInternalBatchNumber) ? item.InternalBatchNumber : item.PreviousInternalBatchNumber;
                        item.InternalBatchNumber = preparationOrderItem.InternalBatchNumber;
                        item.Discount = preparationOrderItem.Discount;
                        item.ExpiryDate = preparationOrderItem.ExpiryDate;
                        item.ExtraDiscount = preparationOrderItem.ExtraDiscount;
                        item.PpaHT = preparationOrderItem.PpaHT;
                        item.VendorBatchNumber = preparationOrderItem.VendorBatchNumber;
                        item.Status = preparationOrderItem.Status == BlStatus.Deleted ? BlStatus.Deleted : BlStatus.Updated;
                    }
                    // changement de la quantité
                    else
                    {
                        var find = request.PreparationOrderItems
                            .FirstOrDefault(c => c.ProductId == item.ProductId && c.InternalBatchNumber == item.InternalBatchNumber);

                        if (find != null)
                        {
                            item.IsControlled = true;
                            item.Quantity = find.Quantity;
                            item.Packing = find.Packing;
                            item.Status = find.Status;
                        }
                    }
                }
                try
                {
                    var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery { OrganizationId = order.CustomerId }, cancellationToken);
                    var zonePicking = request.PreparationOrderItems.First();
                    if (zonePicking != null)
                    {
                        var pickingZoneId = zonePicking.PickingZoneId;
                        var pickingZoneName = zonePicking.PickingZoneName;

                        var response = await ServiceAX2012Factory
                            .Create(_mapper.Map<PreparationOrderDtoV6>(bl), order, customer, pickingZoneId, pickingZoneName, _model.UserAx, _model.PasswordAx)
                            .SaveAsync();
                        if (response.IsValid)
                        {
                            _preparationOrderRepository.Update(bl);
                            var itemToRelease = bl.PreparationOrderItems.Where(c => c.PickingZoneId == pickingZoneId).ToList();
                            var blToRelease = bl.ShallowClone();
                            blToRelease.PreparationOrderItems = itemToRelease;
                            await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                            await ReleaseReservedQuantities(blToRelease, order, customer.Id);
                            await _commandBus.SendAsync(new OrderAxCreatedCommand { CodeAx = response.CodeAx, OrderNumber = order.SequenceNumber }, cancellationToken);
                            // Affect Agent to controlled Zone 
                            await _commandBus.SendAsync(new AddAgentsCommand
                            {
                                PreparationOrderId = request.Id,
                                PickingZoneId = request.PickingZoneId,
                                PickingZoneName = request.PickingZoneName,
                                ExecutedById = request.ExecutedById,
                                ExecutedByName = request.ExecutedByName,
                                VerifiedById = request.VerifiedById,
                                VerifiedByName = request.VerifiedByName
                            }, cancellationToken);
                            await _commandBus.SendAsync(new ControlPreparationOrderCommand { Id = request.Id }, cancellationToken);

                            // End
                        }
                        else return response;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error while controlling " + order.SequenceNumber +
                        " ==> Exception message :" + e.Message + " inner exception message : " + e.InnerException?.Message);
                    var orderValidationErrors = new Dictionary<string, string>();
                    orderValidationErrors.Add("Exception", "Error while controlling " + order.SequenceNumber +
                        " ==> Exception message :" + e.Message + " inner exception message : " + e.InnerException?.Message);
                    return new AxValidationDto
                    {
                        IsValid = false,
                        OrderValidationErrors = orderValidationErrors
                    };
                }
            }
            catch(Exception e)
            {
                _logger.Error("Error while controlling " + request.Id+" : " + e.Message + " inner exception message : " + e.InnerException?.Message);
                var orderValidationErrors = new Dictionary<string, string>();
                orderValidationErrors.Add("Exception", "Error while controlling " + request.Id + " : " + e.Message + " inner exception message : " + e.InnerException?.Message);
                return new AxValidationDto
                {
                    IsValid = false,
                    OrderValidationErrors = orderValidationErrors
                };
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(request.Id.ToString()).Release();                
            }
            return default;

        }

        private async Task ReleaseReservedQuantities(Entities.PreparationOrder bl, OrderDtoV5 order, Guid customerId)
        {
            var command = new ReleaseReservedQuantityCommandV3();
            command.OrganizationId = bl.OrganizationId;
            command.QuantitiesToRelease = new List<ReleaseReservedQuantityCommandV2>();
            var supplierId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            foreach (var item in bl.PreparationOrderItems.Where(x => x.OldQuantity.HasValue && x.OldQuantity > 0))
            {
                if (item.OldQuantity != null)
                {
                    var batchItem = new BatchDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.OldQuantity.Value,
                        InternalBatchNumber = string.IsNullOrEmpty(item.PreviousInternalBatchNumber)
                            ? item.InternalBatchNumber
                            : item.PreviousInternalBatchNumber
                    };

                    await ReleaseQuantitiesAsync(batchItem, supplierId);
                    command.QuantitiesToRelease.Add(new ReleaseReservedQuantityCommandV2
                    {
                        InternalBatchNumber = batchItem.InternalBatchNumber,
                        Quantity = batchItem.Quantity,
                        ProductId = batchItem.ProductId
                    });
                }
            }

            await ReleaseQuotasAsync(bl, order, customerId);

            if (command.QuantitiesToRelease.Any()) await _commandBus.SendAsync(command);
        }

        private async Task ReleaseQuotasAsync(Entities.PreparationOrder bl, OrderDtoV5 order, Guid customerId)
        {
            foreach (var item in bl.PreparationOrderItems.GroupBy(x => x.ProductId))
            {
                var oldQnt = item.Sum(x => x.OldQuantity);
                var sumOfQuantities = item
                    .Where(x=>x.Status != BlStatus.Deleted)
                    .Sum(x => x.Quantity);
                if (oldQnt.HasValue && oldQnt.Value - sumOfQuantities > 0)
                {
                    await IncreaseQuota(customerId, order.CreatedById, item.FirstOrDefault()!.ProductId, oldQnt.Value - sumOfQuantities);
                }
            }
        }


        private async Task ReleaseQuantitiesAsync(BatchDto orderItem, Guid? supplierId)
        {

            var key = orderItem.ProductId.ToString() + supplierId;
            try
            {
                await LockProvider<string>.ProvideLockObject(key).WaitAsync();
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key);
                var index = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x => x.ProductId == orderItem.ProductId
                                   && x.InternalBatchNumber == orderItem.InternalBatchNumber);
                if (index == -1)
                {
                    return;
                }

                if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >= orderItem.Quantity)
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -= orderItem.Quantity;
                else
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;

                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                //throw;
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(key).Release();
            }

        }

        private async Task IncreaseQuota(Guid customerId, Guid salesPersonId, Guid productId, int quantity)
        {
            var supplierId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            if (supplierId != null)
                await _commandBus.SendAsync(new IncreaseQuotaCommandV2
                {
                    ProductId = productId,
                    CustomerId = customerId,
                    SalesPersonId = salesPersonId,
                    Quantity = quantity,
                    OrganizationId = supplierId.Value
                });
        }

        public async Task<ValidationResult> Handle(MakeOrderAsToBeShippedCommand request, CancellationToken cancellationToken)
        {
            var bls = await _preparationOrderRepository.Table 
                .Include(c => c.PreparationOrderItems)
                .Where(c => c.OrderId == request.OrderId)
                .ToListAsync(cancellationToken);
            foreach (var bl in bls)
            {
                bl.PreparationOrderStatus = PreparationOrderStatus.ReadyToBeShipped;
                _preparationOrderRepository.Update(bl);
            }
            await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
            return default;

        }
    }
}
