using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Invoices.Commands;
using GHPCommerce.Core.Shared.Events.DeliveryOrders;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.PreparationOrder.Entities;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NLog;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.PreparationOrder.Consumers
{
    public class DeliveryOrderConsumer : 
        IConsumer<IDeliveryOrderCancelledEvent>,
        IConsumer<DeliveryOrderCreated>
    {
        private readonly IRepository<DeleiveryOrder, Guid> _deliveryRepository;
        private readonly IHubContext<PreparationOrderHub> _hubContext;
        private readonly ICache _redisCache;
        private readonly ICommandBus _commandBus;

        public DeliveryOrderConsumer(Logger logger, 
            IRepository<DeleiveryOrder, Guid> deliveryRepository, 
            IHubContext<PreparationOrderHub> hubContext,
            ICommandBus commandBus,
            ICache redisCache)
        {
            _deliveryRepository = deliveryRepository;
            _hubContext = hubContext;
            _redisCache = redisCache;
            _commandBus = commandBus;
        }

        public async Task Consume(ConsumeContext<IDeliveryOrderCancelledEvent> context)
        {
            var deliveryOrder = await _deliveryRepository.Table
                .Where(c => c.Id == context.Message.DeliveryOrderId)
                .Include(c => c.DeleiveryOrderItems)
                .FirstOrDefaultAsync();
            _deliveryRepository.Delete(deliveryOrder);
            
            var cnxId = _redisCache.Get<string>("op_hub" + context.Message.UserId);

            try
            {
                _deliveryRepository.UnitOfWork.SaveChanges();
                await _commandBus.SendAsync(new DeleteInvoiceCommand {OrderId = deliveryOrder.OrderId});
                await _hubContext.Clients.Client(cnxId).SendAsync("getCreateDeliveryOrderNotification" , "Echec a la creation du BL");        
            }
            catch (Exception e)
            {
                await _hubContext.Clients.Client(cnxId).SendAsync("getCreateDeliveryOrderNotification" , "Echec Rollback BL");
            }
         
        }

        public async Task Consume(ConsumeContext<DeliveryOrderCreated> context)
        {
            try
            {
                var cnxId = _redisCache.Get<string>("op_hub" + context.Message.UserId);
                await _hubContext.Clients.Client(cnxId).SendAsync("getCreateDeliveryOrderNotification", "Creation BL terminée avec succès");
            }
            catch (Exception ex)
            {

            }
        }
    }
}