using System;
using Automatonymous;
using MassTransit.Saga;

namespace GHPCommerce.Notification.Saga
{
    public class DeliveryReceiptState: SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get ; set ; }
        public State CurrentState { get; set; }
        public Guid DeliveryReceiptId { get; set; }
        public int Version { get; set; }
    }
}