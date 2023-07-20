using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Dosages.Commands;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Application.Catalog.Dosages.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Dosages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/dosages")]
    [ApiController]
    public class DosagesController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public DosagesController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<DosageDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetDosagesListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/dosages/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<IEnumerable<DosageDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllDosagesQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/dosages/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<DosageDto> GetDosageById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Dosage id should not be null or empty");
            return _commandBus.SendAsync(new GetDosageByIdQuery { Id = id });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(DosageModel model)
        {
            var createDosageCommand = _mapper.Map<CreateDosageCommand>(model);
            var result = await _commandBus.SendAsync(createDosageCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/dosages/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, DosageModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var updateDosageCommand = _mapper.Map<UpdateDosageCommand>(model);
            var result = await _commandBus.SendAsync(updateDosageCommand);
            return ApiCustomResponse(result);
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<DosageDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetDosagesPagedQuery() { GridQuery = query });
        }
        [HttpDelete]
        [Route("/api/dosages/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            await _commandBus.Send(new DeleteDosageCommand {Id = id});
            return Ok();
        }
    }
}
