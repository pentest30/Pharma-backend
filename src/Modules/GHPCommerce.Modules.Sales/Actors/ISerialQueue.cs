using System;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Sales.Actors
{
    public interface ISerialQueue
    {
        Task Enqueue(Action action);
        Task Enqueue(Func<Task> asyncAction);
        Task<T> Enqueue<T>(Func<T> function);
        Task<T> Enqueue<T>(Func<Task<T>> asyncFunction);
    }
}