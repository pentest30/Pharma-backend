using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.PharmacologicalClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/pharmacologicalClass")]
    [ApiController]
    [Authorize]
    public class PharmacologicalClassController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public PharmacologicalClassController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<PharmacologicalClassDto>> Get(string filter , string sort , int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetPharmacologicalClassesListQuery (filter, sort, page, pageSize));
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<SyncPagedResult<PharmacologicalClassDto>> GetPaged( SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new  GetPharmacologicalClassesPagedQuery {GridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/pharmacologicalClass/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup","TechnicalDirector", "TechnicalDirectorGroup")]

        public Task<IEnumerable<PharmacologicalClassDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllPharmacologicalClassesQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/pharmacologicalClass/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup", "OnlineCustomer")]

        public Task<PharmacologicalClassDto> GetPharmacologicalClassById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Product  class id should not be null or empty");
            return _commandBus.SendAsync(new GetPharmacologicalClassByIdQuery { Id = id });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(PharmacologicalClassModel model)
        {
            var createProductClassCommand = _mapper.Map<CreatePharmacologicalClassCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/pharmacologicalClass/{id:Guid}")]
         [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, PharmacologicalClassModel model)
        {
            if (id != model.Id)
                return BadRequest();

            try
            {
                var updatePharmacologicalClassCommand = _mapper.Map<UpdatePharmacologicalClassCommand>(model);
                var result = await _commandBus.SendAsync(updatePharmacologicalClassCommand);
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
        [Route("/api/pharmacologicalClass/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeletePharmacologicalClassCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
