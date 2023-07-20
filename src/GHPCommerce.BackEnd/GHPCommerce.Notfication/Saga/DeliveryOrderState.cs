using System;
using Automatonymous;
using MassTransit.Saga;

namespace GHPCommerce.Notification.Saga
{
    public class DeliveryOrderState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }
        public State CurrentState { get; set; }
        public Guid DeliveryOrderId { get; set; }
    }
}