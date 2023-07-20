using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Events;
using MediatR;

namespace GHPCommerce.Infra.Mediator.Commands
{
    public class CommandBus :ICommandBus
    {
        private readonly IMediator _mediator;

        public CommandBus(IMediator mediator){
            _mediator = mediator;
        }
        public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
        {
            await _mediator.Send(command, cancellationToken);
        }

        public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> request, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(request, cancellationToken);
        }

        public Task Publish<TEnvent>(TEnvent notification, CancellationToken cancellationToken = default) where TEnvent : IEvent
        {
            return _mediator.Publish(notification, cancellationToken);
        }
    }
}
