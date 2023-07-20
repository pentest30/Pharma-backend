using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.PreparationOrder.Commands.DeleiveryOrder;
using GHPCommerce.Modules.PreparationOrder.DTOs.DeleiveryOrders;
using GHPCommerce.Modules.PreparationOrder.Queries.DeleiveryOrders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.PreparationOrder.Controllers
{
    [Route("api/deleivery-orders")]
    [ApiController]
    public class DeleiveryOrdersController :  ApiController
    {
        private readonly ICommandBus _commandBus;

        /// <inheritdoc />
        public DeleiveryOrdersController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "PrintingAgent","Admin")]
        [Route("/api/deleivery-orders/search")]
        public Task<SyncPagedResult<DeleiveryOrderDto>> Search(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedDeleiveryOrdersQuery { DataGridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "PrintingAgent", "Admin")]
        [Route("/api/deleivery-orders/search-by-barcode")]
        public Task<SyncPagedResult<DeleiveryOrderDto>> SearchByBarCode(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];

            return _commandBus.SendAsync(new GetPagedDeleiveryOrdersQuery { DataGridQuery = query, barCode = barCode, });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "PrintingAgent","Admin")]
        [Route("/api/deleivery-orders")]
        public async Task<ActionResult> Post(CreateDeleiveryOrderCommand command)
        {
            var result =  await  _commandBus.SendAsync(command);
            return ApiCustomResponse(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/deleivery-orders/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "OnlineCustomer", "SalesPerson", "Admin", "PrintingAgent", "Admin")]
        public async Task<DeliveryOrderDtoV2> GetOrderById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Delivery Order id should not be null or empty");
            return await _commandBus.SendAsync(new GetDeliveryOrderByIdQuery() { Id = id });

        }
    }
}