using AutoMapper;
using GHPCommerce.Application.Catalog.ZoneGroups.Commands;
using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Application.Catalog.ZoneGroups.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/zoneGroups")]
    [ApiController]

    public class ZoneGroupsController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public ZoneGroupsController(IMapper mapper, ICommandBus commandBus)
        {
            _mapper = mapper;
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/api/zoneGroups/search")]
        public Task<SyncPagedResult<ZoneGroupDto>> Get(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedZoneGroupQuery { DataGridQuery = query });
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/zoneGroups/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]

        public Task<IEnumerable<ZoneGroupDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllZoneGroupQuery());
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create(CreateZoneGroupCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }

        [HttpPut]
        [Route("/api/zoneGroups/{code}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Update,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update(UpdateZoneGroupCommand command)
        {

            try
            {
                var result = await _commandBus.SendAsync(command);
                return ApiCustomResponse(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/zoneGroups/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteZoneGroupCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

    }
}
