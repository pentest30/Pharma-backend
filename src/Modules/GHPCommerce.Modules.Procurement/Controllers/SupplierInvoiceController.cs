using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Invoices;
using GHPCommerce.Core.Shared.Contracts.SupplierInvoices.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Procurement.Commands.Invoices;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using GHPCommerce.Modules.Procurement.Queries.Invoices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Procurement.Controllers
{
    [Route("api/supplier-invoices")]
    [ApiController]
    public class SupplierInvoiceController :ApiController
    {
        private readonly ICommandBus _commandBus;
     
        public SupplierInvoiceController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
       
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/items/")]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.POST, "Admin",  "Buyer", "BuyerGroup")]
        public async Task<ActionResult> Item(Guid invoiceId, CreateInvoiceItemCommand model)
        {
          
            if (invoiceId != model.InvoiceId) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
            var result = await task.ConfigureAwait(false);
            return Ok(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/update/")] 
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.PUT, "Admin", "Buyer", "BuyerGroup")]
        public async Task<ActionResult> Put(Guid invoiceId, UpdateInvoiceItemCommand model)
        {
          
            if (invoiceId != model.InvoiceId) return new BadRequestResult();
            var task = _commandBus.SendAsync(model);
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/save")]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.PUT, "Admin",  "Buyer", "BuyerGroup")]
        public async Task<ActionResult> SaveInvoice(Guid invoiceId)
        {

            if (invoiceId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new SaveInvoiceCommand { InvoiceId = invoiceId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/validate")]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.PUT, "Admin",  "Buyer", "BuyerGroup")]
        public async Task<ActionResult> ValidateInvoice(Guid invoiceId)
        {

            if (invoiceId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new ValidateInvoiceCommand { InvoiceId = invoiceId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/remove")]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.DELETE, "Admin",  "Buyer", "BuyerGroup")]
        public async Task<ActionResult> DeleteInvoice(Guid invoiceId)
        {

            if (invoiceId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteInvoiceCommand { InvoiceId = invoiceId });
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/supplier-invoices/{invoiceId:Guid}/items/{productId:Guid}/{internalBatch}")]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.DELETE, "Admin",  "Buyer", "BuyerGroup")]
        public async Task<ActionResult> DeleteInvoiceItem([FromRoute] Guid invoiceId , [FromRoute] Guid productId,[FromRoute] string internalBatch)
        {

            if (invoiceId == Guid.Empty) return new BadRequestResult();
            var task = _commandBus.SendAsync(new DeleteInvoiceItemCommand { InvoiceId = invoiceId, ProductId = productId, InternalBatchNumber = internalBatch});
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.GET, "Admin", "InventoryManager", "Buyer", "BuyerGroup")]
        [Route("/api/supplier-invoices/search")]
        public Task<SyncPagedResult<SupplierInvoiceDto>> GetOrdersForWholesaler(SyncDataGridQuery query)
        {
            var validInvoiceRequest = Request.Headers["validInvoice"];
            bool? validInvoice = validInvoiceRequest == "true";
            return _commandBus.SendAsync(new GetPagedSupplierInvoicesQuery { SyncDataGridQuery = query, ValidInvoice = validInvoice});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.GET, "Admin", "InventoryManager", "Buyer", "BuyerGroup")]
        [Route("/api/supplier-invoices/{invoiceId:guid}")]
        public Task<SupplierInvoiceDto> GetInvoiceById( [FromRoute] Guid invoiceId)
        {
            return _commandBus.SendAsync(new GetInvoiceByIdQuery {InvoiceId = invoiceId});
        }
        
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Procurement, PermissionAction.GET, "Admin", "InventoryManager", "Buyer", "BuyerGroup")]
        [Route(template: "/api/supplier-invoices/get-invoice-ref")]
        public Task<SupplierInvoiceDto> GetInvoiceByRefSync(GetInvoiceByDocRefQuery command)
        {
            return _commandBus.SendAsync(command);
        }
    }
}