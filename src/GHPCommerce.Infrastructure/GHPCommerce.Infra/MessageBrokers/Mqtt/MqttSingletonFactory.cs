using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace GHPCommerce.Infra.MessageBrokers.Mqtt
{
   

    public static  class MqttSingletonFactory 
    {
        

      
        public static IMqttClient MqttClient
        {
            get
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var options = new MqttClientOptionsBuilder()
                    .WithClientId("Client1")
                    .WithTcpServer("localhost")
                   .Build();
                var factory = new MqttFactory();
                var mqttClient = factory.CreateMqttClient();
                mqttClient.ConnectAsync(options, CancellationToken.None);
                return mqttClient;
            }
           
        }
    }
}
