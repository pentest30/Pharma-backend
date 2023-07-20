using System;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Sales.Commands.Atom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/onlineOrders")]
    [ApiController]
    public class OnlineOrdersController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        public OnlineOrdersController(ICommandBus commandBus, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
  

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/onlineOrder/{orderId:Guid}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<CachedOrder>> CreateFromHPCS(Guid orderId, OrderCreateCommand model)
        {
            if (orderId != model.OrderId) return BadRequest(null);
            var result = await _commandBus.SendAsync(model);
            if (result == null) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<AtomOrderContract>> Create(OrderCreateCommandV1 model)
        {
            var result = await _commandBus.SendAsync(model);
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<ValidationResult>> Put(OrderEditCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return Ok(result);
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        [Route("/api/onlineorders/{orderId:guid}/{customerCode}/save")]
        public async Task<ActionResult<ValidationResult>> SaveOrder([FromRoute] Guid orderId, [FromRoute] string customerCode)
        {
            var result = await _commandBus.SendAsync(new ValidateOnlineOrderCommand {OrderId = orderId, CustomerCode = customerCode});
            return Ok(result);
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        [Route("/api/onlineorders/{orderId:guid}/{customerCode}/cancel")]
        [HttpDelete]
        public async Task<ActionResult<ValidationResult>> CancelOrder([FromRoute] Guid orderId, [FromRoute] string customerCode)
        {
            var result = await _commandBus.SendAsync(new CancelOnlineOrderCommand {OrderId = orderId, CustomerCode = customerCode});
            return Ok(result);
        }
        #region OrderItem Endpoints ,Param:FEFO=False,
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/onlineOrder/{orderId:Guid}/create")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<CachedOrder>> CreateOnlineOrder(Guid orderId, OrderCreateCommandV2 model)
        {
            if (orderId != model.OrderId) return BadRequest(null);
            var result = await _commandBus.SendAsync(model);
            if (result == null) return BadRequest(result);
            return Ok(result);
        }
        /*
         * Batch Number has to be requested
         */
        #endregion
    }
}