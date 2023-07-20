using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PickingZones.Commands;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Application.Catalog.PickingZones.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.PickingZones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/pickingZones")]
    [ApiController]
    [Authorize]
    public class PickingZonesController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public PickingZonesController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<PickingZoneDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetPickingZonesListQuery(term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/pickingZones/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup","SalesPerson","Supervisor","SalesManager","Controller","Consolidator", "Executer","InventoryManager","PrintingAgent")]

        public Task<IEnumerable<PickingZoneDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllPickingZonesQuery());
        }
        
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(PickingZoneModel model)
        {
            var createProductClassCommand = _mapper.Map<CreatePickingZoneCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/pickingZones/{id:Guid}")]
         [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, PickingZoneModel model)
        {
            if (id != model.Id)
                return BadRequest();

            try
            {
                var updateBrandCommand = _mapper.Map<UpdatePickingZoneCommand>(model);
                var result = await _commandBus.SendAsync(updateBrandCommand);
                return ApiCustomResponse(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<PickingZoneDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedPickingZones() { GridQuery = query });
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/pickingZones/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeletePickingZoneCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
