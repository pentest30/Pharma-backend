using System;
using System.Threading.Tasks;
using Automatonymous;
using GHPCommerce.Core.Shared.Events.CreditNotes;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Notification.Saga
{
    public class CreditNoteStateMachine : MassTransitStateMachine<CreditNoteState>
    {
        public CreditNoteStateMachine()
        {
        
            InstanceState(x => x.CurrentState);
            ConfigureCorrelationIds();
            Initially(
                When(CreditNoteSubmitted)
                    .Then(x => x.Instance.CreditNoteId = x.Data.CreditNoteId) 
                    .ThenAsync(ProvisioningStockCommand)
                    .TransitionTo(Submitted).Finalize()
            );
       
        }

 
        private async Task ProvisioningStockCommand(BehaviorContext<CreditNoteState, ICreditNoteSubmittedEvent> context)
        {
            var uri = new Uri("rabbitmq://localhost/invent-queue");
            var sendEndpoint = await context.GetSendEndpoint(uri);
            var msg = new CreditNoteInventoryMessage
            {
                OrganizationId = context.Data.OrganizationId,
                CorrelationId = context.Data.CorrelationId,
                CreditNoteId = context.Data.CreditNoteId,
                UserId = context.Data.UserId,
                RefDoc = context.Data.RefDoc
            };
            msg.Items.AddRange(context.Data.ItemEvents);
            await sendEndpoint.Send(msg);
        }

        private void ConfigureCorrelationIds()
        {
            Event(() => CreditNoteSubmitted, x => x.CorrelateById(x => x.Message.CorrelationId)
                .SelectId(c => c.Message.CorrelationId)); 

        }
        public State Submitted { get; private set; } 

        public Event<ICreditNoteSubmittedEvent> CreditNoteSubmitted { get; private set; } 
    }
}