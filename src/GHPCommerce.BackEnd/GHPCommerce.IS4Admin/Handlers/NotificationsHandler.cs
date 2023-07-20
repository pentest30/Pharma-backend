using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Events.Guest;
using GHPCommerce.IS4Admin.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace GHPCommerce.IS4Admin.Handlers
{
    public class NotificationsHandler :  INotificationHandler<OrderCreatedEvent>
    {
        private readonly IHubContext<NotificationHub> _context;
        //private readonly ICache _redisCache;
        private readonly IMqttClient _mqttClient;

        public NotificationsHandler(IHubContext<NotificationHub> context)
        {
            _context = context;
           // _redisCache = redisCache;
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client2")
                .WithTcpServer("localhost")
                .Build();
            var factory = new MqttFactory();

            _mqttClient = factory.CreateMqttClient();
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ClientConnected);
            _mqttClient.UseApplicationMessageReceivedHandler(MessageReceived);
            _mqttClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task ClientConnected(MqttClientConnectedEventArgs obj)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("orders/created").Build());
        }

        private Task MessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(arg.ApplicationMessage.Payload));
            throw new System.NotImplementedException();
        }


        private async Task NotifyUserViaSignalR(OrderCreatedEvent request, CancellationToken cancellationToken)
        {
            //foreach (var guid in request.ShoppingCartItemModels.Select(x => x.OrganizationId).Distinct())
            //{
            //    var org = await _redisCache.GetAsync<List<string>>("_signalR_" + guid.Value, cancellationToken);
            //    if (org == null) continue;
            //    foreach (var connectionId in org)
            //        await _context.Clients.Client(connectionId).SendAsync("receiveOrderCreated", request, cancellationToken);
            //}
        }
        public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            return NotifyUserViaSignalR(notification, cancellationToken);
        }
    }
}
