using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.WebApi.Models.Brands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Application.Catalog.Brands.Commands;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.Application.Catalog.Brands.Queries;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using Microsoft.AspNetCore.Authorization;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/brands")]
    [ApiController]
    [Authorize]
    public class BrandsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public BrandsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<PagingResult<BrandDto>> Get(string term, string sort,int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetBrandsListQuery (term,sort, page,pageSize));
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<SyncPagedResult<BrandDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetBrandsPagedQuery() {GridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/brands/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","BuyerGroup", "TechnicalDirectorGroup")]
        public Task<IEnumerable<BrandDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllBrandsQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/brands/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirectorGroup")]
        public Task<BrandDto> GetProductClassById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Brand id should not be null or empty");
            return _commandBus.SendAsync(new GetBrandByIdQuery { Id = id });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create(BrandModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateBrandCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/brands/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Update([FromRoute] Guid id, BrandModel model)
        {
            if (id != model.Id)
                return BadRequest();

            try
            {
                var updateBrandCommand = _mapper.Map<UpdateBrandCommand>(model);
                var result = await _commandBus.SendAsync(updateBrandCommand);
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
        [Route("/api/brands/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteBrandCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
