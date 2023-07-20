using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/suppliers")]
    [ApiController]
    [Authorize]
    public class SuppliersController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        public SuppliersController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "Admin","SalesPerson")]
        public Task<PagingResult<OrganizationDtoV1>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetSuppliersListQuery(term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Tiers, PermissionAction.Read, "Admin")]
        [Route("/api/suppliers/b2b-customer")]
        public Task<IEnumerable<OrganizationDto>> GetAllForB2BCustomer()
        {
            return _commandBus.SendAsync(new GetSuppliersForB2BCustomerListQuery());
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin","Buyer", "BuyerGroup")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/suppliers/post")]
        [HttpPost]
        public async Task<SyncPagedResult<SupplierDto>> CustomersBySalesPerson([FromBody] SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GetPagedSuppliersQuery { GridQuery = query });
        }
        //[ResourceAuthorization(PermissionItem.Tiers, PermissionAction.Read, "SalesPerson", "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/suppliers/{organizationId:Guid}")]
        [HttpGet]
        public async Task<SupplierDto> GetSupplierByOrgId(Guid organizationId)
        {
            return await _commandBus.SendAsync(new GetSupplierByIdQuery { SupplierId = organizationId});
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/suppliers/supplier/{Id:Guid}")]
        [HttpGet]
        public async Task<SupplierDto> GetSupplierById(Guid Id)
        {
            return await _commandBus.SendAsync(new GetByIdOfSupplierQuery { Id = Id});
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/suppliers/{CustomerId:Guid}/all")]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET,  "Admin","Buyer", "OnlineCustomer")]
        public async Task<List<SupplierDto>> GetListOfSupplierByCustomer(Guid CustomerId)
        {
            return await _commandBus.SendAsync(new GetListOfSupplierByCustomerQuery { CustomerId = CustomerId});
        }
    }
}

