using System;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using Microsoft.AspNetCore.SignalR;

namespace GHPCommerce.Modules.Sales.Hubs
{
    public class InventSumHub :Hub, ICommandHandler<InventQuantityChangedCommand, object>
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("client SR connected...");
            return base.OnConnectedAsync();
        }

        public async Task<object> Handle(InventQuantityChangedCommand request, CancellationToken cancellationToken)
        {
            if (Clients == null)
                return default;
            await Clients.All.SendAsync("productQuantityChanged", new {productId=  request.ProductId,quantity =  request.CurrentQuantity }, cancellationToken)
                .ConfigureAwait(false);
            return default;
        }
    }
}
