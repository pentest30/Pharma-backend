using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.Commands.StockState;
using GHPCommerce.Modules.Inventory.DTOs.StockState;
using GHPCommerce.Modules.Inventory.Queries.StockState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Infra.Filters;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/inventory/stock-state")]
    [ApiController]
    public class InventoryStockStateController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventoryStockStateController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/inventory/stock-state/search")]
        public async   Task<SyncPagedResult<DTOs.StockStateDtoV1>> GetStockStates(SyncDataGridQuery query)
        {
            return  await _commandBus.SendAsync( new GetStockStatePagedQuery() { SyncDataGridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/stock-state/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public Task<IEnumerable<StockStateDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllStockStateQuery());
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create(CreateStockStateCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Put(UpdateStockStateCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpDelete("{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            await _commandBus.Send(new DeleteStockStateCommand() { Id = id });
            return Ok();
        }
    }
}
