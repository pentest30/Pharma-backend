using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Lists.Commands;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Application.Catalog.Lists.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/lists")]
    [ApiController]
    [Authorize]
    public class ListsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public ListsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<ListDto>> Get(string term, string sort ,int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetListsListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/lists/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<IEnumerable<ListDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllListQuery());
        }
        [HttpGet]
        [Route("/api/lists/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<ListDto> GetManufacturerById([FromRoute] Guid id)
        {
            if (id == default)
                throw new InvalidOperationException("List id should not be null or empty");

            try
            {
                var listDto = await _commandBus.SendAsync(new GetListByIdQuery { Id = id });
                return listDto;
            }
            catch (Exception e)
            {
                 throw new Exception(e.Message);
            }
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(ListModel model)
        {
            var createListCommand = _mapper.Map<CreateListCommand>(model);
            var result = await _commandBus.SendAsync(createListCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/lists/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
         [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, ListModel model)
        {
            if (id != model.Id)
                return BadRequest();
            try
            {
                var updateLisCommand = _mapper.Map<UpdateLisCommand>(model);
                var result = await _commandBus.SendAsync(updateLisCommand);
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

        public Task<SyncPagedResult<ListDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetListsPagedQuery() { GridQuery = query });
        }

        [HttpDelete]
        [Route("/api/lists/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();
            try
            {
                await _commandBus.Send(new DeleteListCommand() {Id = id});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
