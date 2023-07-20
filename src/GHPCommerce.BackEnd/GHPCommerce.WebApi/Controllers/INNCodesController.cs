using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Application.Catalog.INNCodes.Commands;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Application.Catalog.INNCodes.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.InnCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/innCodes")]
    [ApiController]
    public class INNCodesController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public INNCodesController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<InnCodeDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetInnCodesListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/innCodes/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<IEnumerable<InnCodeDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllInnCodesQuery());
        }
        [HttpGet]
        [Route("/api/innCodes/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<InnCodeDto> GetInnCodeById([FromRoute] Guid id)
        {
            if (id == default)
                throw new InvalidOperationException("INN code id should not be null or empty");

            try
            {
                var innDto = await _commandBus.SendAsync(new GetInnCodeByIdQuery { Id = id });
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
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(INNCodeModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateInnCodeCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/innCodes/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, INNCodeModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var updateInnCodeCommand = _mapper.Map<UpdateInnCodeCommand>(model);
            var result = await _commandBus.SendAsync(updateInnCodeCommand);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/innCodes/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteInnCodeCommand { Id = id });
                return Ok();
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

        public Task<SyncPagedResult<InnCodeDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetInnCodesPagedQuery() { GridQuery = query });
        }
        [HttpPut]
        [Route("/api/innCodes/{id:Guid}/addLine")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, InnCodeLineModel model)
        {
            if (id != model.InnCodeId)
                return BadRequest();
            var updateInnCodeCommand = _mapper.Map<CreateInnCodeLineCommand>(model);
            await _commandBus.Send(updateInnCodeCommand);
            return ApiCustomResponse(new ValidationResult());
        }
    }
}
