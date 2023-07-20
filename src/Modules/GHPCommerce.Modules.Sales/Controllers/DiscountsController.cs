using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.DTOs.Discounts;
using GHPCommerce.Modules.Sales.Queries.Discounts;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/discounts")]
    [ApiController]
    public class DiscountsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public DiscountsController(ICommandBus commandBus, IMapper mapper, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _mapper = mapper;
            _currentUser = currentUser;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesPerson","Buyer", "BuyerGroup")]
        public Task<PagingResult<DiscountDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetDiscountsListQuery(term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesPerson","Supervisor","SalesManager","Buyer", "BuyerGroup")]
        [Route("/api/discounts/active/{productId}")]
        public async Task<IEnumerable<DiscountDto>> GetActiveDiscounts(Guid productId)
        {
           return await _commandBus.SendAsync(new GetActiveDiscountByProductQuery { ProductId = productId});

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/discounts/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "Admin", "SalesPerson","Supervisor","SalesManager","Buyer", "BuyerGroup")]
        public async Task<ActionResult> Create(Discount model)
        {
            var createDiscountCommand = _mapper.Map<CreateDiscountCommand>(model);

            var result = await _commandBus.SendAsync(createDiscountCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/discounts/{id}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "Admin", "SalesPerson","Buyer", "BuyerGroup")]
        public async Task<ActionResult> Update(Discount model)
        {
            var updateDiscountCommand = _mapper.Map<UpdateDiscountCommand>(model);

            var result = await _commandBus.SendAsync(updateDiscountCommand);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/discounts/{productCode}/ax")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "Admin", "SalesPerson","Buyer", "BuyerGroup")]
        public async Task<ActionResult> Create(CreateDiscountAxCommand model)
        {
          var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson","Supervisor","Buyer", "BuyerGroup")]
        [Route("/api/discounts/search")]
        public Task<SyncPagedResult<DiscountDto>> GetDiscountSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedDiscountsQuery { SyncDataGridQuery = query });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin","Buyer", "BuyerGroup", "OnlineCustomer")]
        [Route("/api/discounts/{organizationId}/active")]
        public async Task<List<DiscountDtoV1>> GetAllActiveDiscounts(Guid organizationId)
        {

            return await _commandBus.SendAsync(new GetAllActiveDiscountsQuery { OrganizationId = organizationId});

        }
    }
}
