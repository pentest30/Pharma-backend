using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client.Options;

namespace GHPCommerce.Infra.MessageBrokers
{
    public static class MessageBrokersCollectionExtensions 
    {
       
        public static IServiceCollection AddMqttClientSender(this IServiceCollection services, MessageBrokerOptions config)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(config.MqttOptions.ClientId)
                .WithTcpServer(config.MqttOptions.ServerUrl)
                .Build();
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            mqttClient.ConnectAsync(options, CancellationToken.None);
            _=services.AddSingleton(mqttClient);
            return services;
        }
    }
}
