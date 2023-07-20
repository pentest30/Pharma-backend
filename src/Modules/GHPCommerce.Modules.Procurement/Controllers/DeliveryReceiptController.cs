using System;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Procurement.Commands.Receipts;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Queries.DeliveryReceipts;
using GHPCommerce.Modules.Procurement.Queries.Invoices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Procurement.Controllers
{
    [Route("api/delivery-receipts")]
    [ApiController]
    public class DeliveryReceiptController : ApiController
    {
        private readonly ICommandBus _commandBus;

        public DeliveryReceiptController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{receiptId:Guid}/items/")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> Item(Guid receiptId, CreateReceiptItemCommand model)
        {
          
            if (receiptId != model.DeliveryReceiptId) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
        
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
        
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{receiptId:Guid}/update/")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> Put(Guid receiptId, UpdateReceiptItemCommand model)
        {
          
            if (receiptId != model.DeliveryReceiptId) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
        
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
        
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{receiptId:Guid}/save")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> SaveInvoice(Guid receiptId)
        {
            if (receiptId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new SaveReceiptCommand { Id = receiptId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{receiptId:Guid}/validate")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> ValidateInvoice(Guid receiptId)
        {
            if (receiptId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new ValidateReceiptCommand { Id = receiptId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/delivery-receipts/search")]
        public Task<SyncPagedResult<DeliveryReceiptDto>> GetOrdersForWholesaler(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedDeliveryReceiptsQuery { Query = query });
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{id:Guid}/remove")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteReceipt(Guid id)
        {

            if (id == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteReceiptCommand { Id = id });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/delivery-receipts/{id:Guid}/items/{productId:Guid}/{internalBatch}/{totalAmount}/{receiptsAmountExcTax}")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> DeleteReceiptItem([FromRoute] Guid id , [FromRoute] Guid productId,
            [FromRoute] string internalBatch,[FromRoute] decimal TotalAmount, [FromRoute] decimal ReceiptsAmountExcTax)
        {

            if (id == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteReceiptItemCommand() { Id = id, ProductId = productId, InternalBatchNumber = internalBatch
                , TotalAmount = TotalAmount, ReceiptsAmountExcTax = ReceiptsAmountExcTax });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
    }
}