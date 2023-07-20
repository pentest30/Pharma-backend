using System;
using System.Threading.Tasks;
using Automatonymous;
using GHPCommerce.Core.Shared.Events.DeliveryOrders;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Notification.Saga
{
    public class DeliveryOrderStateMachine: MassTransitStateMachine<DeliveryOrderState>
    {
        public DeliveryOrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            ConfigureCorrelationIds();
            Initially(
                When(DeliveryOrderSubmitted)
                    .Then(x => x.Instance.DeliveryOrderId = x.Data.DeliveryOrderId)
                    .ThenAsync(DecreaseStockCommand)
                    .TransitionTo(Submitted)
            );
            DuringAny( When(DeliveryOrderCancelled) 
                .TransitionTo(Rejected)
                .Finalize());
            During(Submitted,
                When(DeliveryOrderCompleted)
                    .ThenAsync(DeliveryOrderCompletedCommand)
                    .TransitionTo(Completed)
                    .Finalize());
        }

        private async Task DecreaseStockCommand(BehaviorContext<DeliveryOrderState, IDeliveryOrderSubmittedEvent> ctx)
        {
            var uri = new Uri("rabbitmq://localhost/invent-queue");
            var sendEndpoint = await ctx.GetSendEndpoint(uri);
            var msg = new InventoryDecreaseMessage
            {
                OrganizationId = ctx.Data.OrganizationId,
                CorrelationId = ctx.Data.CorrelationId,
                DeliveryOrderId = ctx.Data.DeliveryOrderId,
                UserId = ctx.Data.UserId,
                RefDoc = ctx.Data.RefDoc,
                DeliveryOrder = ctx.Data.DeliveryOrder,
                Order = ctx.Data.Order,
                OpItems = ctx.Data.OpItems,
            };
            //msg.Items.AddRange(ctx.Data.ItemEvents);
            await sendEndpoint.Send(msg);        
        }

        private async Task DeliveryOrderCompletedCommand(BehaviorContext<DeliveryOrderState, IDeliveryOrderCompletedEvent> ctx)
        {
            var uri = new Uri("rabbitmq://localhost/preparation-order-queue");
            var sendEndpoint = await ctx.GetSendEndpoint(uri);
            var msg = new DeliveryOrderCreated
            {
                UserId = ctx.Data.UserId
            };
            
            await sendEndpoint.Send(msg);
        }

        private void ConfigureCorrelationIds()
        {
            Event(() => DeliveryOrderSubmitted, x => x.CorrelateById(x => x.Message.CorrelationId)
                .SelectId(c => c.Message.CorrelationId));
            Event(() => DeliveryOrderCancelled, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => DeliveryOrderCompleted, x => x.CorrelateById(x => x.Message.CorrelationId));
        }
        
        public State Submitted { get; private set; }
        public State Rejected { get; private set; }
        public State Completed { get; private set; }

        public Event<IDeliveryOrderSubmittedEvent> DeliveryOrderSubmitted { get; private set; }
        public Event<IDeliveryOrderCancelledEvent> DeliveryOrderCancelled { get; private set; }
        public Event<IDeliveryOrderCompletedEvent> DeliveryOrderCompleted{ get; private set; }
    }
}