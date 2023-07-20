using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Events.Guest;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Services;
using GHPCommerce.IS4Admin.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace GHPCommerce.IS4Admin.Services
{
    public class MqttService : IMqttService
    {
        private readonly IHubContext<NotificationHub> _context;
        private readonly ICache _redisCache;
        private readonly IConfiguration _configuration;
        private  IMqttClient _mqttClient;

        public MqttService(IHubContext<NotificationHub> context, ICache redisCache, IConfiguration configuration)
        {
            _context = context;
            _redisCache = redisCache;
            _configuration = configuration;
        }
        private async Task ClientConnected(MqttClientConnectedEventArgs obj) 
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("orders/created").Build());
        }
        private async Task MessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payload = JsonConvert.DeserializeObject<OrderCreatedEvent>(Encoding.UTF8.GetString(arg.ApplicationMessage.Payload));
            await NotifyUserViaSignalR(payload, CancellationToken.None);
        }

        public async Task StartAsync()
        {
            var url = _configuration.GetValue<string>("MessageBroker:MqttOptions:ServerUrl");
            var clientId = _configuration.GetValue<string>("MessageBroker:MqttOptions:ClientId");

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(url)
                .Build();
            var factory = new MqttFactory();

            _mqttClient = factory.CreateMqttClient();
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ClientConnected);
            _mqttClient.UseApplicationMessageReceivedHandler(MessageReceived);
            await _mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        private async Task NotifyUserViaSignalR(IGuestCreatedEvent request, CancellationToken cancellationToken)
        {
            foreach (var guid in request.ShoppingCartItemModels.Select(x => x.OrganizationId).Distinct())
            {
                var org = await _redisCache.GetAsync<Dictionary<string, string>>("_signalR_" + guid.Value, cancellationToken);
                if (org == null) continue;
                foreach (var connectionId in org.Values)
                    await _context.Clients.Client(connectionId).SendAsync("receiveOrderCreated", request, cancellationToken);
            }
        }
    }
}
