using MediatR;

namespace GHPCommerce.Domain.Domain.Commands
{
    public interface  ICommandHandler<in T> : IRequestHandler<T> where T : ICommand {}
    public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : ICommand<TResponse> { } 


}
