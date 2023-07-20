using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.DTOs.Invent;
using GHPCommerce.Modules.Inventory.Queries.Invent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/invents/")]
    [ApiController]
    public class InventController  :ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        [Route("/api/invents/search")]
        public Task<SyncPagedResult<InventDtoV1>> GetInventSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetInventsPagedQuery { SyncDataGridQuery = query });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        [Route("/api/invents/{zoneId:guid}/{stockStateId:guid}")]
        public Task<IEnumerable<InventDtoV1>> GetInventByZone([FromRoute] Guid zoneId,[FromRoute] Guid stockStateId)
        {
            return _commandBus.SendAsync(new GetInventByZoneQuery { ZoneId = zoneId, StockStateId = stockStateId});
        }
    }
}