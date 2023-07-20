using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Infra.MessageBrokers;
using MassTransit;

namespace GHPCommerce.Modules.Sales.Consumers
{
    public class SalesOrderConsumer : IConsumer<AtomOrderContract>
    {
        private readonly MessageBrokerOptions _options;

        public SalesOrderConsumer(MessageBrokerOptions options)
        {
            _options = options;
        }
        public Task Consume(ConsumeContext<AtomOrderContract> context)
        {
            throw new System.NotImplementedException();
        }
    }
}