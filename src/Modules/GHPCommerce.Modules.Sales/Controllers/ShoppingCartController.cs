using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.Modules.Sales.ShoppingCarts.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/baskets")]
    [ApiController]
    public class ShoppingCartController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public ShoppingCartController(ICommandBus commandBus, IMapper mapper, ICurrentUser currentUser, ICache redisCache)
        {
            _commandBus = commandBus;
            _mapper = mapper;
            _currentUser = currentUser;
            _redisCache = redisCache;
          
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> Put(ShoppingCartItemModel model)
        {
            if (_currentUser.UserId == Guid.Empty)
            {
                
                var shoppingCart = _redisCache.Get<ShoppingCartModel>("_cart" +model.CartId);
                if (shoppingCart != null)
                    shoppingCart.ShoppingCartItems.Add(model);
                else
                    shoppingCart = new ShoppingCartModel {CartId = model.CartId, ShoppingCartItems = new List<ShoppingCartItemModel> {model}};

                await _redisCache.AddOrUpdateAsync<ShoppingCartModel>("_cart" + model.CartId, shoppingCart);
                return Created($"/api/baskets/{model.ProductId}", model);
            }

            model.CustomerId = _currentUser.UserId;
            var createCommand = _mapper.Map<CreateShoppingCartItemCommand>(model);
            var result = await _commandBus.SendAsync(createCommand, CancellationToken.None);
            return ApiCustomResponse(result);
        }

       
    }
}
