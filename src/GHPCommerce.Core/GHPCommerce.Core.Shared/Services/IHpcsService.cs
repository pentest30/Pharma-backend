using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using RestSharp;

namespace GHPCommerce.Core.Shared.Services
{
    public interface IHpcsService
    {
       
        /// <summary>
        /// creates new online order 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<IRestResponse<AtomOrderContract>> CreateOrderAsync(AtomOrderContract @message);
        /// <summary>
        /// edit online pending order
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<IRestResponse<EditOrderContract>> EditOrderAsync(EditOrderContract @message);
        /// <summary>
        /// validates the online order
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ErrorValidationContract> ValidateOrderAsync(ValidateOnlineOrderContract context);
        /// <summary>
        /// cancel the online order
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ErrorValidationContract> CancelOrderAsync(CancelOnlineOrderContract context);
    }
}