using GHPCommerce.Core.Shared.Contracts.Cache;
using MassTransit;
using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Services;
using HPCS.Service.OptionConfiguration;
using Serilog.Core;

namespace HPCS.Service.Consumers
{

    public class SalesOrderConsumer :  IConsumer<AtomOrderContract>,
        IConsumer<ValidateOnlineOrderContract>,
        IConsumer<CancelOnlineOrderContract>,
        IConsumer<EditOrderContract>
    {
        private static  AppSettings? _appSettings;
        private readonly IHpcsService _hpcsService;
        private readonly Logger _logger;

        public SalesOrderConsumer(AppSettings? appSettings,
            IHpcsService hpcsService,
            Logger logger)
        {
            _appSettings = appSettings;
            _hpcsService = hpcsService;
            _logger = logger;
        }
   
        public async Task Consume(ConsumeContext<AtomOrderContract> context)
        {
            try
            {
                var orderResponse = await _hpcsService.CreateOrderAsync(context.Message);
                if (orderResponse.IsSuccessful)
                {
                    var uri = new Uri($"rabbitmq://{_appSettings?.MessageBroker.RabbitMq.Url}/order-created");
                    var sendEndpoint = await context.GetSendEndpoint(uri);
                    await sendEndpoint.Send(orderResponse.Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
        }
        public async Task Consume(ConsumeContext<ValidateOnlineOrderContract> context)
        {
            try
            {
                var orderResponse = await _hpcsService.ValidateOrderAsync(context.Message);
                var uri = new Uri($"rabbitmq://{_appSettings?.MessageBroker.RabbitMq.Url}/order-created");
                var sendEndpoint = await context.GetSendEndpoint(uri);
                orderResponse.Action = (uint)ActionType.Save;
                await sendEndpoint.Send(orderResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
        }


        public async Task Consume(ConsumeContext<CancelOnlineOrderContract> context)
        {
            try
            {
                var orderResponse = await _hpcsService.CancelOrderAsync(context.Message);
                var uri = new Uri($"rabbitmq://{_appSettings?.MessageBroker.RabbitMq.Url}/order-Canceled");
                var sendEndpoint = await context.GetSendEndpoint(uri);
                orderResponse.Action =(uint) ActionType.Cancel;
                await sendEndpoint.Send(orderResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
        }

        public async Task Consume(ConsumeContext<EditOrderContract> context)
        {
            try
            {
                var orderResponse = await _hpcsService.EditOrderAsync(context.Message);
                var uri = new Uri($"rabbitmq://{_appSettings?.MessageBroker.RabbitMq.Url}/order-edited");
                var sendEndpoint = await context.GetSendEndpoint(uri);
                await sendEndpoint.Send(orderResponse.Data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
        }
    }
}