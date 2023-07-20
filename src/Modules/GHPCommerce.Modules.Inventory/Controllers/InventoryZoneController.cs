using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.Commands.Zones;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.DTOs.Zone;
using GHPCommerce.Modules.Inventory.Queries.Zones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/inventory/zones")]
    [ApiController]
//    [Authorize]
    public class InventoryZoneController:ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventoryZoneController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/inventory/zone-types/search")]
        public Task<SyncPagedResult<ZoneTypeDto>> GetZoneTypes(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetZoneTypesQuery { SyncDataGridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/inventory/zones/search")]
        public Task<SyncPagedResult<StockZoneDto>> GetZones(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetStockZonePagedQuery { SyncDataGridQuery = query });
        }
       
        [HttpPost]
        [Route("/api/inventory/zone-types")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create(CreateZoneTypeCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Route("/api/inventory/zones")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> CreateZone(CreateStockZoneCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/inventory/zones/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> CreateZone([FromRoute] Guid id,UpdateStockZoneCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/inventory/zone-types")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Put(UpdateZoneTypeCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpDelete()]
        [Consumes("application/json")]
        [Route("/api/inventory/zone-types/{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Delete([FromRoute]Guid id)
        {
            await _commandBus.Send(new DeleteZoneTypeCommand() {Id = id});
            return Ok();
        }
        [HttpDelete]
        [Consumes("application/json")]
        [Route("/api/inventory/zones/{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "TechnicalDirectorGroup")]
        public async Task<ActionResult> DeleteZone([FromRoute] Guid id)
        {
            await _commandBus.Send(new DeleteStockZoneCommand { Id = id });
            return Ok();
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/inventory/zones/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public Task<IEnumerable<ZoneDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllZoneQuery());
        }
    }
}
