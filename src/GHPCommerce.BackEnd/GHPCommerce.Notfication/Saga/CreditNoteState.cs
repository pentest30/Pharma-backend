using System;
using Automatonymous;
using MassTransit.Saga;

namespace GHPCommerce.Notification.Saga
{
    public class CreditNoteState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public State CurrentState { get; set; }
        public Guid CreditNoteId { get; set; }
        public int Version { get; set; }
    }
}