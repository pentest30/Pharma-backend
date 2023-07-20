using MediatR;

namespace GHPCommerce.Domain.Domain.Events
{
    public interface IEventHandler<in T> : INotificationHandler<T> where T : IEvent
    {}
}