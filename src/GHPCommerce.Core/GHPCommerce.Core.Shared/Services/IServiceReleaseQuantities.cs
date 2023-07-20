using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Core.Shared.Services
{
    public interface IServiceReleaseQuantities
    {
        Task ReleaseQuantities(Guid productId, string internalBatchNumber, int quantity , Guid supplierId);

       
    }
}