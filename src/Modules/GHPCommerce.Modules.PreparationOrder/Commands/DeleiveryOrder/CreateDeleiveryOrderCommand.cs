using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Core.Shared.Contracts.SalesInvoice;
using GHPCommerce.Core.Shared.Contracts.Transactions;
using GHPCommerce.Core.Shared.Events.DeliveryOrders;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.PreparationOrder.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.PreparationOrder.Commands.DeleiveryOrder
{
    public class CreateDeleiveryOrderCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }

    public class CreateDeleiveryOrderCommandHandler : ICommandHandler<CreateDeleiveryOrderCommand, ValidationResult>
    {
        private readonly IRepository<Entities.DeleiveryOrder, Guid> _deleiveryOrderRepository;
        private readonly IRepository<Entities.PreparationOrder, Guid> _preparationOrderRepository;
        private readonly IRepository<ConsolidationOrder, Guid> _consolidationOrderRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly Logger _logger;
        private readonly ISequenceNumberService<Entities.DeleiveryOrder, Guid> _sequenceNumberService;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _bus;
        private double _tax;


        public CreateDeleiveryOrderCommandHandler(
            IRepository<Entities.DeleiveryOrder, Guid> deleiveryOrderRepository,
            IRepository<ConsolidationOrder, Guid> consolidationOrderRepository,
            IRepository<Entities.PreparationOrder, Guid> preparationOrderRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            IPublishEndpoint bus,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            Logger logger,
            ISequenceNumberService<Entities.DeleiveryOrder, Guid>  sequenceNumberService)
        {
            _deleiveryOrderRepository = deleiveryOrderRepository;
            _preparationOrderRepository = preparationOrderRepository;
            _consolidationOrderRepository = consolidationOrderRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _logger = logger;
            _sequenceNumberService = sequenceNumberService;
            _mapper = mapper;
            _bus = bus;

        }

        public async Task<ValidationResult> Handle(CreateDeleiveryOrderCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");
            var consolidationOrder = await _consolidationOrderRepository.Table.Where(c => c.OrderId == request.OrderId)
                .FirstOrDefaultAsync(cancellationToken);
            var order = await _commandBus.SendAsync(new GetSharedOrderById { OrderId = request.OrderId }, cancellationToken);
            var generatedDeliveryOrder =
                await _deleiveryOrderRepository.Table.FirstOrDefaultAsync(c => c.OrderId == request.OrderId,
                    cancellationToken);
            if (generatedDeliveryOrder != null)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Bl Already Generated", "Already Generated") } };
                return validations;
            }
            var result = await _preparationOrderRepository.Table
                .Include(c => c.PreparationOrderItems)
                .Where(c => c.OrderId == request.OrderId && c.OrganizationId == orgId)
                .SelectMany(c => c.PreparationOrderItems)
                .ToListAsync(cancellationToken);
            var operationOrdersItems = result.Where(c => c.Status != BlStatus.Deleted).AsEnumerable();
            var deliveryOrder = new Entities.DeleiveryOrder();

            try
            {

                await LockProvider<string>.WaitAsync(consolidationOrder.OrderIdentifier + orgId, cancellationToken);
                deliveryOrder.Validated = true;
                deliveryOrder.CustomerId = order.CustomerId.Value;
                deliveryOrder.CustomerName = order.CustomerName;
                deliveryOrder.SupplierId = order.SupplierId.Value;
                deliveryOrder.OrderDate = consolidationOrder.OrderDate.Value;
                deliveryOrder.TotalPackage = consolidationOrder.TotalPackage;
                deliveryOrder.TotalPackageThermolabile = consolidationOrder.TotalPackageThermolabile;
                deliveryOrder.DeleiveryOrderDate = DateTime.Now.Date;
                deliveryOrder.OrderIdentifier = order.OrderNumberSequence.ToString();
                deliveryOrder.CodeAx = order.CodeAx;
                deliveryOrder.OrganizationId = consolidationOrder.OrganizationId;
                deliveryOrder.OrderId = consolidationOrder.OrderId;
                var keysq = nameof(Entities.DeleiveryOrder) + orgId;
                await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(deliveryOrder.DeleiveryOrderDate, orgId.Value);
                deliveryOrder.SequenceNumber = sq;
                LockProvider<string>.Release(keysq);
                var items = operationOrdersItems.GroupBy(c => c.InternalBatchNumber).Select(c =>
                {
                    var preparationOrderItem = c.First();
                    var orderItem = order.OrderItems.Find(orderItem => orderItem.ProductId == c.First().ProductId);
                    return new DeleiveryOrderItem
                    {
                        Discount = preparationOrderItem.Discount,
                        ExtraDiscount = orderItem.ExtraDiscount,
                        ExpiryDate = preparationOrderItem.ExpiryDate,
                        UnitPrice = orderItem.UnitPrice,
                        PurchaseUnitPrice = orderItem.PurchaseUnitPrice,
                        PpaHT = preparationOrderItem.PpaHT,
                        Tax = orderItem.Tax,
                        ProductCode = preparationOrderItem.ProductCode,
                        ProductName = preparationOrderItem.ProductName,
                        ProductId = preparationOrderItem.ProductId,
                        Quantity = c.Sum(ele => ele.Quantity),
                        InternalBatchNumber = c.Key,
                        VendorBatchNumber = preparationOrderItem.VendorBatchNumber,
                        PFS = orderItem.PFS,
                        Packing = preparationOrderItem.Packing,
                    };
                });
                deliveryOrder.DeleiveryOrderItems = new List<DeleiveryOrderItem>(items);
                _deleiveryOrderRepository.Add(deliveryOrder);
                await _deleiveryOrderRepository.UnitOfWork.SaveChangesAsync();
                await _bus.Publish<IDeliveryOrderSubmittedEvent>(new
                {
                    DeliveryOrderId = deliveryOrder.Id,
                    CorrelationId = Guid.NewGuid(),
                    ItemEvents = _mapper.Map<List<DeleiveryOrderItem>>(deliveryOrder.DeleiveryOrderItems),
                    DeliveryOrder = deliveryOrder,
                    Order = order,
                    OpItems = _mapper.Map<List<GHPCommerce.Core.Shared.Events.PreparationOrder.PreparationOrderItem>>(result),
                    OrganizationId = orgId,
                    Userid = _currentUser.UserId,
                    RefDoc = deliveryOrder.SequenceNumber
                }, cancellationToken);
                
            }
            catch (Exception e)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", e.Message) } };
                if (consolidationOrder != null)
                    LockProvider<string>.Release(consolidationOrder.OrderIdentifier + orgId);
                return validations;
            }
            finally
            {
                if (consolidationOrder != null)
                {
                 
                    LockProvider<string>.Release(consolidationOrder.OrderIdentifier + orgId);
                    await _commandBus.SendAsync(new CreateInvoiceCommand() { DeliveryOrderId = deliveryOrder.Id }, cancellationToken);
   
                }
            }
            return default;
        }
    }
}