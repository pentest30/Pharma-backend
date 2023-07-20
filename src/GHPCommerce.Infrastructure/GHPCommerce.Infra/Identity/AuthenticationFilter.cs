using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.PipeConfigurators;

namespace GHPCommerce.Infra.Identity
{
    public class AuthenticationFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("authenticationFilter");
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var token = context.Headers.Where(x => x.Key == "token").Select(x => x.Value.ToString()).Single();

            // TODO: Call token introspection

            await next.Send(context);
        }
    }
    public class AuthenticationFilterSpecification<T> : IPipeSpecification<ConsumeContext<T>> where T : class
    {
        public void Apply(IPipeBuilder<ConsumeContext<T>> builder)
        {
            var filter = new AuthenticationFilter<T>();
            builder.AddFilter(filter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }

    public class AuthenticationFilterConfigurationObserver : ConfigurationObserver, IMessageConfigurationObserver
    {
        public AuthenticationFilterConfigurationObserver(IConsumePipeConfigurator receiveEndpointConfigurator) : base(receiveEndpointConfigurator)
        {
            Connect(this);
        }

        public void MessageConfigured<TMessage>(IConsumePipeConfigurator configurator)
            where TMessage : class
        {
            var specification = new AuthenticationFilterSpecification<TMessage>();
            configurator.AddPipeSpecification(specification);
        }
    }

    public static class AuthenticationExtensions
    {
        public static void UseAuthenticationFilter(this IConsumePipeConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            _ = new AuthenticationFilterConfigurationObserver(configurator);
        }
    }
}