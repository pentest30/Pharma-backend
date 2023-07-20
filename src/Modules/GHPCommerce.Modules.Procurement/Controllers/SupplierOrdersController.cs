using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Procurement.Commands.Orders;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Queries.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Procurement.Controllers
{
    [Route("api/supplier-orders")]
    [ApiController]
    public class SupplierOrdersController : ApiController
    {
        private readonly ICommandBus _commandBus;

        public SupplierOrdersController(ICommandBus commandBus)
        {
            _commandBus = commandBus;

        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/items/")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult>
            Item([FromRoute] Guid orderId, CreateSupplierOrderItem model)
        {

            if (orderId != model.OrderId) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);

            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> UpdateOrder([FromRoute] Guid orderId, UpdateSupplierOrderItem model)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
            var result = await task.ConfigureAwait(false);
            Console.WriteLine();
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/save")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> SaveOrder(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new SaveSupplierOrderCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/items/{productId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteOrderItem( [ FromRoute]Guid orderId, [FromRoute]Guid productId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteSupplierOrderItemCommand { OrderId = orderId, ProductId = productId});
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeletePendingOrder( [ FromRoute]Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeletePendingSupplierOrderCommand { OrderId = orderId});
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        } 
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/remove-saved")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteOrder( [ FromRoute]Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteSupplierOrderCommand { OrderId = orderId});
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/validate")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> ValidateOrder(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new ValidateSupplierOrderCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/reject")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> RejectOrder(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new RejectSupplierOrderCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/cancel")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> CancelOrder(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new CancelOrderCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/back-to-saved")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> BackToSavedStatus(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new ReturnToSavedStatusCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-orders/{orderId:Guid}/complete-status")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> FinishOrder(Guid orderId)
        {

            if (orderId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new FinishSupplierOrderCommand { OrderId = orderId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "Buyer", "BuyerGroup")]
        [Route("/api/supplier-orders/search")]
        public Task<SyncPagedResult<SupplierOrderDto>> GetOrdersForWholesaler([FromBody] SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedSupplierOrdersQuery { SyncDataGridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/supplier-orders/print")]
        public Task<ValidationResult> PrintOrder(PrintSupplierOrderCommand query)
        {
            return _commandBus.SendAsync(query);
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //  [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Read, "SalesPerson")]
        [Route("/api/supplier-orders/{supplierId:Guid}/all")]
        public async Task<IEnumerable<SupplierOrderDto>> GetAllValidOrders([FromRoute] Guid supplierId)
        {
            return await _commandBus.SendAsync(new GetValidSupplierOrders { SupplierId = supplierId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //  [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Read, "SalesPerson")]
        [Route("/api/supplier-orders/{supplierId:Guid}/complete")]
        public async Task<IEnumerable<SupplierOrderDto>> GetAllCompletedOrders([FromRoute] Guid supplierId)
        {
            return await _commandBus.SendAsync(new GetCompleteSupplierOrders { SupplierId = supplierId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/supplier-orders/{organizationId:Guid}/pending-orders")]
        //  [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Read, "OnlineCustomer", "SalesPerson","Admin")]
        public async Task<ActionResult<SupplierOrderDto>> GetPendingSuppliersOrders([FromRoute] Guid organizationId)
        {
            var result = await _commandBus.SendAsync(new GetAllPendingOrdersQuery());
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/supplier-orders/{orderId:guid}")]
        public Task<SupplierOrderDto> GetInvoiceById([FromRoute] Guid orderId)
        {
            return _commandBus.SendAsync(new GetSupplierOrderByIdQuery { OrderId = orderId });
        }
    }
}