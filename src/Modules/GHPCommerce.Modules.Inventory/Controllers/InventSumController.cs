
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.Commands;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.Models.InventSum;
using GHPCommerce.Modules.Inventory.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/inventory/inventsum")]
    [ApiController]
    //[Authorize]
    public class InventSumController: ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventSumController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager", "SalesPerson")]
        [Route("/api/inventory/inventsum/details/{sort}/{page:int}/{pageSize:int}")]
        public async Task<PagingResult<InventSumDto>> Get([FromRoute] string sort, [FromRoute] int page, [FromRoute] int pageSize, [FromBody] GetInventSumListQuery filters)
        {
            if (filters != null)
            {
                filters.Page = page;
                filters.PageSize = pageSize;
                filters.SortDir = !string.IsNullOrEmpty(sort) ? sort.Split('_')[1] : string.Empty;
                filters.SortProp = !string.IsNullOrEmpty(sort) && !sort.Contains("undefined") ? sort.Split('_')[0].UppercaseFirst() : string.Empty;
                return await _commandBus.SendAsync(filters);
            }

            return null;

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson", "Supervisor", "InventoryManager")]
        [Route("/api/inventory/inventsum/search")]
        public Task<SyncPagedResult<InventSumDto>> GetInventSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedInventQuery { SyncDataGridQuery = query });
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        public async Task<InventSumDto> GetById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("InventSum Id should not be null nor empty");
            return await _commandBus.SendAsync(new GetInventSumByIdQuery { Id = id });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/{supplierId:Guid}/{productId:Guid}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson", "Supervisor", "InventoryManager")]
        public async Task<IEnumerable<InventSumDtoV1>> GetByProductId([FromRoute] Guid supplierId,[FromRoute] Guid productId)
        {
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product Id should not be null nor empty");
            return await _commandBus.SendAsync(new GetStockForB2BCustomerQuery { ProductId = productId, SupplierId = supplierId});
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/bydimension")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        public async Task<InventSumDto> GetByDimension([FromBody] InventoryDimensionExistsQuery dimension)
        {
            if (dimension == null ||dimension.OrganizationId==null ||dimension.ProductId==null)
                throw new InvalidOperationException("Invalid inventory dimension");
            return  await _commandBus.SendAsync(new GetInventSumByDimensionQuery { Dimension = dimension });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.POST, "Admin","InventoryManager")]
        public async Task<ActionResult> Create(InventSumModel model)
        {
            var createCommand = _mapper.Map<CreateInventSumCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/inventory/ax")]
        //[ResourceAuthorization(PermissionItem.Inventory, PermissionAction.PUT, "Admin", "InventoryManager")]
        public async Task<ActionResult> Post(InventSumModelV2 model)
        {
            var createCommand = _mapper.Map<CreateAXInventSumCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/inventory/{productCode}/ax")]
        //[ResourceAuthorization(PermissionItem.Inventory, PermissionAction.PUT, "Admin", "InventoryManager")]
        public async Task<ActionResult> Put(UpdateOnHandQuantityCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/inventory/{productCode}/block-activate/ax")]
        //[ResourceAuthorization(PermissionItem.Inventory, PermissionAction.PUT, "Admin", "InventoryManager")]
        public async Task<ActionResult> Put(ActivateBlockInventSumCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/inventory/inventsum/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.PUT, "Admin","InventoryManager")]
        public async Task<ActionResult> Put([FromRoute] Guid id, UpdateInventSumCommand model)
        {
            // ReSharper disable once ComplexConditionExpression
            if (id == Guid.Empty || model.Id != id)
                return BadRequest(); 
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/inventory/inventsum/feed/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.PUT, "Admin", "InventoryManager")]
        public async Task<ActionResult> Feed([FromRoute] Guid id, [FromBody] FeedInventoryCommand model)
        {
            // ReSharper disable once ComplexConditionExpression
            if (id == Guid.Empty || model.Id != id)
                return BadRequest();
            var createCommand = _mapper.Map<FeedInventoryCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }
        
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.DELETE, "Admin", "InventoryManager")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
               var r=  await _commandBus.SendAsync(new DeleteInventSumCommand{ Id = id });
                return ApiCustomResponse(r);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/salesperson/{supplierId:Guid}/{productId:Guid}")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "SalesPerson", "InventoryManager","Supervisor","SalesManager")]
        public async Task<List<CachedInventSum>> GetStockForSalesPerson([FromRoute] Guid supplierId, Guid productId)
        {
            if (supplierId == Guid.Empty || productId == Guid.Empty)
                throw new InvalidOperationException("Id should not be null nor empty");
            return await _commandBus.SendAsync(new GetStockForSalesPerson { SupplierId = supplierId,ProductId = productId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/{supplierId:Guid}/products-for-quota")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "SalesPerson", "Buyer", "Supervisor","SalesManager")]
        public async Task<IEnumerable<InventSumQuotaDto>> GetProductsForQuota( [FromRoute]Guid supplierId)
        {
            return await _commandBus.SendAsync(new GetListOfProductForQuotaQuery());
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/{productCode}/invent/ax")]
        //[ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin","SalesPerson", "Buyer","Supervisor","SalesManager")]
        public async Task<double> GetReservedQuantity( GetReservedQuantityAxQuery command)
        {
            return await _commandBus.SendAsync(command);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/inventory/preparationOrder/search")]
        public Task<SyncPagedResult<InventItemTransactionDto>> GetInventPreparationOrderSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedInventTransactionQuery { SyncDataGridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/byproduct/{supplierId:Guid}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson", "Supervisor", "InventoryManager")]
        public async Task<List<InventSumDtoV1>> GetQuantityByProductIds([FromRoute] Guid supplierId, [FromBody] List<string> productCodes)
        {
            if (productCodes==null || productCodes.Count==0)
                throw new InvalidOperationException("product code list should not be null nor empty");
            return await _commandBus.SendAsync(new GetStockByProductQuery { ProductCodes = productCodes, SupplierId = supplierId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/inventory/inventsum/preparation/{supplierId:Guid}/{productCode}/{zoneName}")]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "SalesPerson", "InventoryManager", "Supervisor", "SalesManager")]
        public async Task<List<InventSumDto>> GetStockForPreparation([FromRoute] Guid supplierId, string productCode, string zoneName)
        {

            if (supplierId == Guid.Empty || string.IsNullOrEmpty(productCode))
                throw new InvalidOperationException("Id should not be null nor empty");
            return await _commandBus.SendAsync(new GetStockForPreparation { SupplierId = supplierId, ProductCode = productCode, ZoneName = zoneName });
        }


    }
}
