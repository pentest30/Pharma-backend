using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.SalesInvoice;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web; 
using GHPCommerce.Modules.Sales.Commands.CreditNotes;
using GHPCommerce.Modules.Sales.Commands.Invoices;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Queries.Invoices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/invoices")]
    [ApiController]
    public class InvoicesController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public InvoicesController(ICommandBus commandBus, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/search")]
        public Task<SyncPagedResult<InvoiceDto>> GetOrdersForWholesaler(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedInvoicesQuery { SyncDataGridQuery = query });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
         [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/{id:GUID}")]
        public async Task<InvoiceDto> GetById(Guid id)
        {
            return await _commandBus.SendAsync(new GetInvoiceByIdQuery { Id = id });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/invoices/{deliveryOrderId}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "Admin", "SalesPerson")]
        public async Task<ActionResult> Create(Guid deliveryOrderId)
        {
            var result = await _commandBus.SendAsync(new CreateInvoiceCommand { DeliveryOrderId = deliveryOrderId});
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin")]
        [Route("/api/invoices/print/{id:Guid}/")]
        public async Task<ActionResult> PrintSalesInvoice([FromRoute] Guid id )
        {
            var result = await _commandBus.SendAsync(new PrintSalesInvoiceCommand { Id = id});
            return Ok();

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent")]
        [Route("/api/invoices/bulkPrint")]
        public async Task<ActionResult> PrintBulkBl(PrintBulkInvoicesCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok();

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/invoices/{invoiceId:Guid}/create-creditnote")]
 
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "ClaimManager")]

        public async Task<ActionResult> CreateCreditNote(Guid invoiceId, [FromBody] CreateCreditNoteCommand command)
        {
            if (command.InvoiceId != invoiceId)
                return BadRequest();
            var result = await _commandBus.SendAsync(command);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/{customerId:Guid}/customer")]
        public Task<SyncPagedResult<InvoiceDto>> GetInvoicesByCustomer(SyncDataGridQuery query, Guid customerId)
        {
            return _commandBus.SendAsync(new GetInvoiceByCustomerIdQuery { SyncDataGridQuery = query , CustomerId = customerId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/{customerId:Guid}/trunover")]
        public Task<decimal> GetInvoicesTurnoverByCustomer([FromRoute] Guid customerId,string start, string end)
        {
            DateTime? s = default;
            DateTime? e = default;
            if (!string.IsNullOrEmpty(start) && start != "null")
                s = DateTime.Parse(start);
            if (!string.IsNullOrEmpty(end) && end != "null")
                e = DateTime.Parse(end);
            return _commandBus.SendAsync(new GetInvoicesTurnOverByCustomerQuery { CustomerId = customerId, Start = s, End = e});
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "Buyer",
            "BuyerGroup", "Admin")]
        [Route("/api/invoices/{customerId:Guid}/export-excel")]
        public async Task<ActionResult> GetCustomersInvoicesAsExcel([FromRoute] Guid customerId, string start, string end)
        {
            DateTime? s = default;
            DateTime? e = default;
            if (!string.IsNullOrEmpty(start) && start != "null")
                s = DateTime.Parse(start);
            if (!string.IsNullOrEmpty(end) && end != "null")
                e = DateTime.Parse(end);
            var file = await _commandBus.SendAsync(new ExportInvoicesByCustomerAsExcelCommand
                { CustomerId = customerId, Start = s, End = e });
            string excelName = $"quota-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            return PhysicalFile(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "Buyer",
            "BuyerGroup", "Admin")]
        [Route("/api/invoices/{customerId:Guid}/export-products-excel")]
        public async Task<ActionResult> GetCustomersProductsAsExcel([FromRoute] Guid customerId, string start, string end)
        {
            DateTime? s = default;
            DateTime? e = default;
            if (!string.IsNullOrEmpty(start) && start != "null")
                s = DateTime.Parse(start);
            if (!string.IsNullOrEmpty(end) && end != "null")
                e = DateTime.Parse(end);
            var file = await _commandBus.SendAsync(new ExportInvoicesItemsByCustomerAsExcelCommand
                { CustomerId = customerId, Start = s, End = e });
            string excelName = $"quota-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            return PhysicalFile(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "Buyer",
            "BuyerGroup", "Admin")]
        [Route("/api/invoices/export-products-excel")]
        public async Task<ActionResult> GetAllInvoicesProductsAsExcel( DateTime? start, DateTime? end)
        {
            DateTime? s = default;
            DateTime? e = default;
           
            var file = await _commandBus.SendAsync(new ExportInvoicesItemsByCustomerAsExcelCommand
                { CustomerId = null, Start = start, End = end });
            string excelName = $"quota-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            return PhysicalFile(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "SalesPerson", "Buyer","BuyerGroup", "Admin")]
        [Route("/api/invoices/{customerId:Guid}/customer-debt")]
        public async Task<IEnumerable<DebtDto>> GetCustomerDebt([FromRoute] Guid customerId)
        {
            return await _commandBus.SendAsync(new GetDebtByCustomerQuery { CustomerId = customerId });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "SalesPerson", "Buyer","BuyerGroup", "Admin")]
        [Route("/api/invoices/{customerId:Guid}/customer-debt-details")]
        public async Task<SyncPagedResult<DebtDetailDto>> GetCustomerDebtDetails([FromRoute] Guid customerId,[FromHeader] int year,[FromHeader] int month, SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GetDetailsDebtByCustomerQuery { CustomerId = customerId, SyncDataGridQuery = query, Year = year, Month = month});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "SalesPerson", "Buyer","BuyerGroup", "Admin")]
        [Route("/api/invoices/{customerCode}/customer-debt-details")]
        public async Task<SyncPagedResult<DebtDetailDto>> GetCustomerDebtDetailsByCode([FromRoute] string customerCode, SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GetDetailsDebtByCustomerQueryV1 { CustomerCode = customerCode, SyncDataGridQuery = query});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "SalesPerson", "Buyer","BuyerGroup", "Admin")]
        [Route("/api/invoices/all/customer-debt")]
        public async Task<SyncPagedResult<DebtDetailDto>> GetAllDebtDetails(SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GePagedDebtByCustomerQuery {  SyncDataGridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "SalesPerson", "Buyer","BuyerGroup", "Admin")]
        [Route("/api/invoices/all-turnovers")]
        public async Task<Dictionary<string, List<TurnoverDto>>> GetTurnovers( string period)
        {
            DateTime? start = default;
            if (!string.IsNullOrEmpty(period) && !string.IsNullOrWhiteSpace(period))
                start = DateTime.Parse(period);
            return await _commandBus.SendAsync(new GetTurnoversForAllCustomersQuery {Period = start} );
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/{customerId:Guid}/sales-log")]
        public Task<SyncPagedResult<InvoiceDtoV1>> GetInvoicesForSalesPersonByCustomer([FromBody] SyncDataGridQuery query, [FromRoute] Guid customerId)
        {
            DateTime? start = default;
            DateTime? end = default;
            var qSTart = Request.Headers["start"];
            var qEnd = Request.Headers["end"];
            if (qSTart != "null"&& qSTart != "{}" &&  qSTart.ToString() != "")
                start = DateTime.Parse(qSTart);
            if (qEnd!="null" && qEnd != "{}" &&   qEnd.ToString() != "")
                end = DateTime.Parse(qEnd);
            return _commandBus.SendAsync(new GetAllInvoicesForSalesPersonByCustomerQuery { SyncDataGridQuery = query , CustomerId = customerId , End = end, Start = start});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/{customerId:Guid}/sales-product-log")]
        public Task<SyncPagedResult<SalesLogByProductDto>> GetInvoicesForSalesPersonByProduct([FromBody] SyncDataGridQuery query, [FromRoute] Guid customerId)
        {
            DateTime? start = default;
            DateTime? end = default;
            var qSTart = Request.Headers["start"];
            var qEnd = Request.Headers["end"];
            if (qSTart != "null"&& qSTart != "{}" &&  qSTart.ToString() != "")
                start = DateTime.Parse(qSTart);
            if (qEnd!="null" && qEnd != "{}" &&   qEnd.ToString() != "")
                end = DateTime.Parse(qEnd);
            return _commandBus.SendAsync(new GetAllProductsByCustomerForSalesPersonQuery { SyncDataGridQuery = query , CustomerId = customerId, Start = start, End = end});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/invoices/reporting/sales-product-log")]
        public Task<SyncPagedResult<SalesLogByProductDto>> GetInvoicesProductForSalesPerson([FromBody] SyncDataGridQuery query)
        {
            DateTime? start = default;
            DateTime? end = default;
            var qSTart = Request.Headers["start"];
            var qEnd = Request.Headers["end"];
            if (qSTart != "null"&& qSTart != "{}" &&  qSTart.ToString() != "")
                start = DateTime.Parse(qSTart);
            if (qEnd!="null" && qEnd != "{}" &&   qEnd.ToString() != "")
                end = DateTime.Parse(qEnd);
            return _commandBus.SendAsync(new GetAllInvoiceProductsForSalesPersonQuery { SyncDataGridQuery = query ,  Start = start, End = end});
        }
    }
}