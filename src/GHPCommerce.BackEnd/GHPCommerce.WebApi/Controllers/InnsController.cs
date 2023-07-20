using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNs.Commands;
using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.Application.Catalog.INNs.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.INNs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/inns")]
    [ApiController]
    [Authorize]
    public class InnsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InnsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<InnDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetINNsListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/inns/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<IEnumerable<InnDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllInnsQuery());
        }
        [HttpGet]
        [Route("/api/inns/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<InnDto> GetInnById([FromRoute] Guid id)
        {
            if (id == default)
                throw new InvalidOperationException("INN id should not be null or empty");

            try
            {
                var innDto = await _commandBus.SendAsync(new GetInnByIdQuery { Id = id });
                return innDto;
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
        public async Task<ActionResult> Create(INNModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateInnCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/inns/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, INNModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var updateInnCommand = _mapper.Map<UpdateInnCommand>(model);
            var result = await _commandBus.SendAsync(updateInnCommand);
            return ApiCustomResponse(result);
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<InnDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetInnPagedQuery() { GridQuery = query });
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inns/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteInnCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
