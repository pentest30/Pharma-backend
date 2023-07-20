using System.Threading.Tasks;

namespace GHPCommerce.Core.Shared.Contracts.Orders
{
    public interface ISaveOrderOnExternalService<T> where  T : class
    {
        /// <summary>
        /// saves sales order on ax2012
        /// </summary>
        /// <returns></returns>
        public Task<T> SaveAsync();
    }

    public interface IValidateOrderOnExternalService
    {
        /// <summary>
        /// validates sales order
        /// </summary>
        /// <returns></returns>
        public  Task ValidateAsync();
    }
}