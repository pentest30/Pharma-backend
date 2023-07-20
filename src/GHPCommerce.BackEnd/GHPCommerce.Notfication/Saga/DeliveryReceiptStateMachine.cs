using System;
using System.Threading.Tasks;
using Automatonymous;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Notification.Saga
{
    public class DeliveryReceiptStateMachine : MassTransitStateMachine<DeliveryReceiptState>
    {
        public DeliveryReceiptStateMachine()
        {
        
            InstanceState(x => x.CurrentState);
            ConfigureCorrelationIds();
            Initially(
                When(DeliveryReceiptSubmitted)
                    .Then(x => x.Instance.DeliveryReceiptId = x.Data.DeliveryReceiptId)
                    //.Then(x => _logger.Info($" Receipt {x.Instance.DeliveryReceiptId} submitted"))
                    .ThenAsync(ProvisioningStockCommand)
                    .TransitionTo(Submitted)
            );
            DuringAny( When(DeliveryReceiptCancelled) 
                //.Then(x => _logger.Error($" Receipt {x.Instance.DeliveryReceiptId} rejected"))
                .TransitionTo(Rejected)
                .Finalize());
            During(Submitted,
                When(DeliveryReceiptCompleted)
                    .ThenAsync(DeliveryReceiptCompletedCommand)
                    .TransitionTo(Completed)
                    .Finalize());
        }

        private async Task DeliveryReceiptCompletedCommand(BehaviorContext<DeliveryReceiptState, IDeliveryReceiptCompletedEvent> context)
        {
            var uri = new Uri("rabbitmq://localhost/procurement-queue");
            var sendEndpoint = await context.GetSendEndpoint(uri);
            var msg = new DeliveryReceiptCreated
            {
                UserId = context.Data.UserId
            };
            
            await sendEndpoint.Send(msg);
        }

        private async Task ProvisioningStockCommand(BehaviorContext<DeliveryReceiptState, IDeliveryReceiptSubmittedEvent> context)
        {
            var uri = new Uri("rabbitmq://localhost/invent-queue");
            var sendEndpoint = await context.GetSendEndpoint(uri);
            var msg = new InventoryMessage
            {
                OrganizationId = context.Data.OrganizationId,
                CorrelationId = context.Data.CorrelationId,
                DeliveryReceiptId = context.Data.DeliveryReceiptId,
                UserId = context.Data.UserId,
                RefDoc = context.Data.RefDoc
            };
            msg.Items.AddRange(context.Data.ItemEvents);
            await sendEndpoint.Send(msg);
        }

        private void ConfigureCorrelationIds()
        {
            Event(() => DeliveryReceiptSubmitted, x => x.CorrelateById(x => x.Message.CorrelationId)
                .SelectId(c => c.Message.CorrelationId));
             Event(() => DeliveryReceiptCancelled, x => x.CorrelateById(x => x.Message.CorrelationId));
             Event(() => DeliveryReceiptCompleted, x => x.CorrelateById(x => x.Message.CorrelationId));

        }
        public State Submitted { get; private set; }
        public State Rejected { get; private set; }
        public State Completed { get; private set; }

        public Event<IDeliveryReceiptSubmittedEvent> DeliveryReceiptSubmitted { get; private set; }
        public Event<IdeliverReceiptCancelledEvent> DeliveryReceiptCancelled { get; private set; }
        public Event<IDeliveryReceiptCompletedEvent> DeliveryReceiptCompleted{ get; private set; }
    }
}