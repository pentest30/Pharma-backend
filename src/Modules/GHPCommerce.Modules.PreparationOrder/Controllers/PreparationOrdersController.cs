using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.PreparationOrder.Commands;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Modules.Shared.Contracts.PreparationOrder.Queries;
using System.Linq;

namespace GHPCommerce.Modules.PreparationOrder.Controllers
{
    [Route("api/preparationOrders")]
    [ApiController]
    public class PreparationOrdersController : ApiController
    {
        private readonly ICommandBus _commandBus;

        public PreparationOrdersController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent","Admin", "Consolidator")]
        [Route("/api/preparationOrders/search")]
        public Task<SyncPagedResult<PreparationOrdersDto>> Get(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];
            return _commandBus.SendAsync(new GetPagedPreparationOrdersQuery { DataGridQuery = query, barCode = barCode, });
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/control/search")]
        public Task<SyncPagedResult<PreparationOrderDtoV4>> GetOpController(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];
            return _commandBus.SendAsync(new GetPagedOpForControllerQuery { DataGridQuery = query,BarCode = barCode, });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin")]
        [Route("/api/preparationOrders/{barcode}/barcode")]
        public Task<PreparationOrdersDtoValidation> GetOpByBarcode(string barcode)
        {
            return _commandBus.SendAsync(new GetOpByBarcodeQuery { BarCode = barcode, });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin","Consolidator")]
        [Route("/api/preparationOrders/controlled/{barcode}/barcode")]
        public Task<PreparationOrdersControlValidationDto> GetControlledOpByBarcode(string barcode)
        {
            return _commandBus.SendAsync(new GetControlledOpByBarcodeQuery { BarCode = barcode, });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/archive/search")]
        public Task<SyncPagedResult<PreparationOrderDtoV4>> GetOpArchived(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];
            return _commandBus.SendAsync(new GetArchivedBlByOrderQuery { DataGridQuery = query, barCode = barCode, });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/consolidation/search")]
        public Task<SyncPagedResult<PreparationOrdersDto>> GetOpConsolidation(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];
            return _commandBus.SendAsync(new GetPagedOpForConsolidationQuery { DataGridQuery = query, barCode = barCode, });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/preparationOrders/{id:Guid}/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "OnlineCustomer", "SalesPerson", "Admin", "PrintingAgent", "Admin", "Consolidator")]
        public Task<PreparationOrdersDtoV2> GetOrderById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Preparation Order id should not be null or empty");
            return _commandBus.SendAsync(new GetPreparationOrderByIdQuery { Id = id });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/print")]
        public async Task<ActionResult> Print(PrintPreparationOrderCommand command)
        {
            var result = await _commandBus.SendAsync(command); 
                return Ok(result); 

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/printBl/{id:Guid}/")]
        public async Task<ActionResult> PrintBl([FromRoute] Guid id )
        {
            var result = await _commandBus.SendAsync(new PrintBlCommand { Id = id}); 
            return Ok(result); 

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/generatePdfBl/{id:Guid}/")]
        public async Task<ActionResult> GeneratePdf([FromRoute] Guid id )
        {
            var result = await _commandBus.SendAsync(new GenerateOpPDFCommand { Id = id}); 
            return Ok(result); 

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Consolidator")]
        [Route("/api/preparationOrders/printBulkBl")]
        public async Task<ActionResult> PrintBulkBl(PrintBulkBlCommand command)
        {
            var result = await _commandBus.SendAsync(command); 
            return Ok(result); 

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Consolidator")]
        [Route("/api/preparationOrders/printBulkPendingBl")]
        public async Task<ActionResult> PrintBulkPendingBl(PrintBulkPendingCommand command)
        {
            var result = await _commandBus.SendAsync(command); 
            return Ok(result); 

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson","Controller", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/{id:Guid}")]
        public async Task<ActionResult> Put(UpdatePreparationOrderCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok(result);

        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/order/{orderId:Guid}/pickingZone/{pickingZoneId:Guid}")]
        public async Task<PreparationOrderDtoV3> GetBlByOrder(Guid orderId,Guid pickingZoneId)
        {
            return await _commandBus.SendAsync(new GetControlledBlByOrderQuery { OrderId = orderId,PickingZoneId=pickingZoneId});

        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin","Controller","Consolidator")]
        [Route("/api/preparationOrders/addOpAgents")]
        public async Task<ActionResult> AddOpAgents(AddAgentsCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok(result);

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/consolidate")]
        public async Task<ActionResult> Consolidate(ZoneConsolidationCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok(result);

        }


        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/AllOpByOrder/{orderId:Guid}")]
        public async Task<List<PreparationOrderDtoV5>> GetAllOpByOrder(Guid orderId)
        {
            return await _commandBus.SendAsync(new GetStateBlByOrderQuery { OrderId = orderId });

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin", "Consolidator")]
        [Route("/api/preparationOrders/{Id}/update-item/{preparationOrderItemId}")]
        public async Task<ActionResult> UpdateItem(UpdatePreparationOrderItem command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "PrintingAgent", "Admin","Controller", "Consolidator")]
        [Route("/api/preparationOrders/{Id}/control-op")]
        public async Task<ActionResult> ControlOp( Guid id)
        {
            var result = await _commandBus.SendAsync(new ControlPreparationOrderCommand {Id = id});
            return Ok(result);

        }
    }
}
