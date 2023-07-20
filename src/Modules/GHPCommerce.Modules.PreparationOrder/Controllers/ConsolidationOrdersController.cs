using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.PreparationOrder.Commands.Consolidation;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Queries.Consolidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Modules.PreparationOrder.Queries.DynamicAx;
using ServiceReference1;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.Controllers
{
    [Route("api/consolidationOrders")]
    [ApiController]
    public class ConsolidationOrdersController : ApiController
    {
        private readonly ICommandBus _commandBus;

        public ConsolidationOrdersController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/consolidationOrders/search")]
        public Task<SyncPagedResult<ConsolidationOrdersDto>> Get(SyncDataGridQuery query)
        {
            var barCode = Request.Headers["barCode"];
            return _commandBus.SendAsync(new GetPagedConsolidationOrdersQuery { barCode = barCode,DataGridQuery = query });
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "Controller", "Admin")]
        [Route("/api/consolidationOrders/{barcode}/barcode")]
        
        public Task<ConsolidationValidationDto> GetConsolidationOrder(string barCode)
        {
            return _commandBus.SendAsync(new GetConsolidationOrderQuery { BarCode = barCode });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Add(ConsolidationCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            if(result!=null && result.IsValid)
            return Ok(result);
            return BadRequest(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult> Update(ConsolidationUpdateCommand command)
        {
            var result = await _commandBus.SendAsync(command);
            return Ok(result);

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/consolidationOrders/{id:Guid}/")]
        public Task<ConsolidationOrder> Get([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Consolidation Order id should not be null or empty");
            return _commandBus.SendAsync(new GetConsolidationOrderByIdQuery { Id = id });

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/consolidationOrders/byOrder/{orderId:Guid}/")]
        public Task<ConsolidationOrder> GetConsolidationByOrder([FromRoute] Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new InvalidOperationException(" Order id should not be null or empty");
            return _commandBus.SendAsync(new GetConsolidationOrderByOrderIdQuery { OrderId = orderId });

        }
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/octet-stream")]

        [Route("/api/consolidationOrders/print/{id:Guid}")]

        public async Task<ActionResult> PrintCommands(Guid id)
        {
            var result = await _commandBus.SendAsync(new PrintConsolidationOrderLabelCommand { Id = id });
            if (!result.IsValid) return Ok(result);
            return Ok();


        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer", "SalesPerson", "Controller", "Admin")]
        [Route("/api/consolidationOrders/packages/ax")]
        public Task<ConsolidationOrderAxDtoV1> AxGetTotalPackages(AxGetTotalPackagesQuery query)
        {
            return _commandBus.SendAsync(query);
        }
        public class DTO
        {
            public string CodeAx { get; set; }
            public  int TotalPackage { get; set; }
            public int TotalPackageThermolabile { get; set; }
        }
        [HttpPost]
        [Route("/api/invoice/ax")]
        public async Task<IActionResult> InvoiceAx(DTO dto)
        {
            try
            {
                DOSI_SalesOrderServiceClient client = new DOSI_SalesOrderServiceClient();
                CallContext callContext = new CallContext();
                callContext.Company = "HP";
                client.ClientCredentials.Windows.ClientCredential.UserName = "khaireddine.mansouri";
                client.ClientCredentials.Windows.ClientCredential.Password = "@Zz@ba2023"; // Code société dans AX
                var msg = await client.invoiceAsync(callContext, dto.CodeAx, dto.TotalPackage,
                     dto.TotalPackageThermolabile);
                // Voir les messages d'erreur
                if (msg.response!.comments != null)
                {
                    var errorMsg = "";
                    foreach (KeyValuePair<int, String> m in msg.response.comments)
                    {
                        errorMsg += m.Value + "\r";
                    }

                    throw new InvalidOperationException(errorMsg);

                }
                return Ok();

            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

    }
}
