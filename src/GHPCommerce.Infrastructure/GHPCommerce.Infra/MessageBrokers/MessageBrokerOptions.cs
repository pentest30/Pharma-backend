using GHPCommerce.Infra.MessageBrokers.Mqtt;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;

namespace GHPCommerce.Infra.MessageBrokers
{
    public class MessageBrokerOptions
    {
        public MqttOptions MqttOptions { get; set; }
        public RabbitMqOptions RabbitMq { get; set; }
    }
}
