using System;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.Commands.TransferLogs;
using GHPCommerce.Modules.Inventory.DTOs.TransferLogs;
using GHPCommerce.Modules.Inventory.Queries.TransferLogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/transfer-logs/")]
    [ApiController]
    public class TransferLogController : ApiController
    {
        private readonly ICommandBus _commandBus;

        public TransferLogController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        [Route("/api/transfer-logs/search")]
        public Task<SyncPagedResult<TransferLogDto>> GetInventSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetTransferLogsPagedQuery { SyncDataGridQuery = query });
        }
        [HttpPost]
        [Route("/api/transfer-logs/")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> CreateTransferLog(TransferLogCreateCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> UpdateTransferLog([FromRoute] Guid logId, TransferLogUpdateCommand model)
        {
            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}/save")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> SaveTransferLog(Guid logId)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new SaveTransferLogCommand { Id = logId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

            
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}/validate")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> ValidateTransferLog(Guid logId)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new ValidateTransferLogCommand { Id = logId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

            
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}/back")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> BackToSaveTransferLog(Guid logId)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new BackToSaveTransferLogCommand { Id = logId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
        }

        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}/items/{productId:Guid}/{internalBatch}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteTransferLogItem([FromRoute] Guid logId, [FromRoute] Guid productId, [FromRoute] string internalBatch)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteTransferLogItemCommand { Id = logId, ProductId = productId, InternalBatchNumber = internalBatch });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteTransferLog([FromRoute] Guid logId)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteTransferLogCommand { Id = logId});
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/transfer-logs/{logId:Guid}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> GetTransferLog([FromRoute] Guid logId)
        {

            if (logId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new GetTransferLogByIdQuery { Id = logId});
            var result = await task.ConfigureAwait(false);
            return Ok(result);
        }
    }
}