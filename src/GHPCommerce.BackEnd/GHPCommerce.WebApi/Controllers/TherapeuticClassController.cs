using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TherapeuticClasses.Commands;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.Application.Catalog.TherapeuticClasses.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.TherapeuticClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/therapeuticClass")]
    [ApiController]
    public class TherapeuticClassController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public TherapeuticClassController(IMapper mapper, ICommandBus commandBus)
        {
            _mapper = mapper;
            _commandBus = commandBus;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<TherapeuticClassDto>> Get(String filer, string sort,int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetTherapeuticClassesListQuery (filer, sort, page, pageSize));
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<SyncPagedResult<TherapeuticClassDto>> GetPaged( SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new  GetTherapeuticClassesPagedQuery {DataGridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/therapeuticClass/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<TherapeuticClassDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllTherapeuticClassesQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/therapeuticClass/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<TherapeuticClassDto> GetTherapeuticClassById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Product  class id should not be null or empty");
            return _commandBus.SendAsync(new GetTherapeuticClassByIdQuery { Id = id });

        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(TherapeuticClassModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateTherapeuticClassCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/therapeuticClass/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Update([FromRoute] Guid id, TherapeuticClassModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var updatePackagingCommand = _mapper.Map<UpdateTherapeuticClassCommand>(model);
            var result = await _commandBus.SendAsync(updatePackagingCommand);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/therapeuticClass/{id:Guid}")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteTherapeuticClassCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
