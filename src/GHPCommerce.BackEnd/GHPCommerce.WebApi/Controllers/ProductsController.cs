using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Brands.Queries;
using GHPCommerce.Application.Catalog.Products.Commands;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/catalog")]
    [ApiController]
    //[Authorize]
    public class ProductsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly Logger _logger;

        public ProductsController(ICommandBus commandBus, IMapper mapper, Logger logger)
        {
            _commandBus = commandBus;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/catalog/products/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup","SalesPerson")]
        public Task<PagingResult<ProductDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetProductListQuery(term, sort, page, pageSize));
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/products/search")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup", "SalesPerson")]
        public Task<SyncPagedResult<ProductDto>> Get(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedProductListQuery { GridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/catalog/{catalogId:Guid}/products/{manufacturerId:Guid?}")]
        [AllowAnonymous]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup")]
        public async Task<CatalogFullDto> Get([FromRoute] Guid catalogId, Guid?manufacturerId, string term, string sort, int page, int pageSize)
        {
            var products = await _commandBus.SendAsync(new GetListOfProductsByCatalogQuery(term, sort, page, pageSize) {CatalogId = catalogId});
            var brands =await _commandBus.SendAsync(new GetBrandsByCatalogIdQuery {CatalogId = catalogId});
            return  new CatalogFullDto {Brands = brands.Distinct(), PagingResult = products};
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/catalog/{catalogId:Guid}/products/search")]
        [AllowAnonymous]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup")]
        public Task<IEnumerable<ProductDtoV3>> Get([FromRoute] Guid catalogId,string query)
        {
            return _commandBus.SendAsync(new GetListOfProductsByCatalogAndNameQuery { CatalogId = catalogId , SearchName = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/products/getall")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "Admin", "InventoryManager")]
        public async Task<IEnumerable<ProductDtoV3>> GetProducts()
        {
            return await _commandBus.SendAsync(new GetProductSharedQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/products/{supplierId:Guid}/b2b-customer")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "Admin")]
        public async Task<PagingResult<ProductDtoV3>> GetProductsForB2BCustomer([FromRoute] Guid supplierId,string term, Guid? productId, int page, int pageSize)
        {
            return await _commandBus.SendAsync(new GetProductListForB2BCustomerQuery { SupplierId = supplierId, Term = term, ProductId = productId, page = page, pageSize = pageSize }) ;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/catalog/{id:Guid}/products/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST, "BuyerGroup", "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create([FromRoute] Guid id, ProductModel model)
        {
            if (id != model.ProductClassId)
                return BadRequest();
            var createProductClassCommand = _mapper.Map<CreateDraftProductCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
       

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "BuyerGroup","TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, [FromRoute] Guid productId, ProductModel model)
        {
            if (productId != model.Id)
                return BadRequest();
            var createProductClassCommand = _mapper.Map<UpdateProductCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }

        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteProductCommand {Id = productId});
                return Ok();
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
        [Route("/api/products/{code}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] string code)
        {
            if (code == string.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteProductByCodeCommand { Code = code });
                return Ok();
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
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/get")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup","OnlineCustomer")]
        public Task<ProductDtoV2> GetDProductById([FromRoute] Guid id, [FromRoute] Guid productId)
        {
          
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");
            return _commandBus.SendAsync(new GetProductByIdQuery {Id = productId, CatalogId = id});

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/ecom")]
        public Task<ProductDtoV2> GetDProductByIdForEcom([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("catalog id should not be null or empty");
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");
            return _commandBus.SendAsync(new GetProductByIdQuery { Id = productId, CatalogId = id });

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{productCode}/products/{innCodeId:Guid}/equivalence")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "OnlineCustomer")]

        public Task<IEnumerable<ProductDtoV5>> GetEquivalencesProduct([FromRoute] string productCode, [FromRoute] Guid innCodeId)
        {
            if (innCodeId == Guid.Empty)
                throw new InvalidOperationException("inncode id should not be null or empty");
            if (String.IsNullOrEmpty(productCode))
                throw new InvalidOperationException("product code should not be null or empty");
            return _commandBus.SendAsync(new GetEquivalencesProductQuery { ProductCode = productCode, InnCodeId = innCodeId });

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/validate")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup" , "BuyerGroup")]
        public async Task<IActionResult> ValidateProduct([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("catalog id should not be null or empty");
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");
            
            return ApiCustomResponse(await _commandBus.SendAsync(new ValidateProductCommand { Id = productId, CatalogId = id }));

        }

        [HttpPost]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/upload")]
        public async Task<ActionResult> UploadImages([FromRoute] Guid id, [FromRoute] Guid productId)
        {

            var files = Request.Form.Files;
            if (id== Guid.Empty)
                throw new InvalidOperationException("catalog id shouldn't be null or empty");
            if (productId== Guid.Empty)
                return BadRequest();
            var productDtoV2 = await _commandBus.SendAsync(new GetProductByIdQuery { Id = productId, CatalogId = id });
            if (productDtoV2 == null)
                return BadRequest("product wasn't found");
            if (files.Count <= 0)
                return BadRequest("Unsuccessful");
            var imageCommands = CreateImageCommandAsync(files);
            var createImagesCommand = new CreateListImagesCommand
            {
                Id = productDtoV2.Id,
                ImageCommands = imageCommands.ToList()
            };
            await _commandBus.Send(createImagesCommand);
            return Ok();
        }
        public static IEnumerable<CreateImageCommand> CreateImageCommandAsync(IFormFileCollection filesUploaded)
        {
            foreach (var file in filesUploaded)
            {
                if (file.Length <= 0) continue;
                var imageModel = new CreateImageCommand();
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    imageModel.ImageBytes = ms.ToArray();
                    imageModel.ImageTitle = file.FileName;
                    yield return imageModel;
                }
            }

        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/products/sales-person")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "SalesPerson", "Buyer", "BuyerGroup", "Admin","SalesManager", "Supervisor", "OnlineCustomer")]
        public async Task<IEnumerable<ProductDtoV5>> GetProductsForSalesPerson( [FromHeader] bool? isPsy,[FromHeader] string term = null)
        {
            return await _commandBus.SendAsync(new GetProductListForSalesPersonQuery { SalesPersonId = null, SearchBy = term, IsPsy = isPsy});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/products/cached/{salesPersonId:guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "SalesPerson", "Buyer", "BuyerGroup", "Admin","SalesManager", "Supervisor","OnlineCustomer")]
        public async Task<SyncPagedResult<ProductDtoV6>> GetPagedCachedProduct(SyncDataGridQuery query, [FromRoute] Guid salesPersonId)
        {
            var supplierOrganizationId = Request.Headers["supplierOrganizationId"];
            String term = Request.Headers["term"];
            var ids = (Request.Headers["classIds"].Count > 0) ? Request.Headers["classIds"].First().Split(',') : null;
            List<Guid> classIds = default;
            if (ids != null)
                classIds = ids.Select(c => (String.IsNullOrEmpty(c)) ? Guid.Empty : Guid.Parse(c)).ToList();
            return await _commandBus.SendAsync(new GetPagedCachedProductQuery { GridQuery = query, SalesPersonId = salesPersonId, Term = term, ClassIds = classIds, SupplierOrganizationId = new Guid(supplierOrganizationId)});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/products/ax/add-update")]
      //  [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Create, "TechnicalDirectorGroup", "BuyerGroup")]
        public async Task<IActionResult> AddOrUpdateAXProduct(AddOrUpdateAXProductCommand command)
        {
          
            return ApiCustomResponse(await _commandBus.SendAsync(command));

        }
      [HttpGet]
      [Consumes("application/json")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [Route("/api/catalog/products/last-code")]
      [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup", "SalesPerson")]
      public async Task<string> GetProductsLastCode()
      {
          return await _commandBus.SendAsync(new GetLastProductCodeUQuery ());
      }
      [HttpGet]
      [Consumes("application/json")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [Route("/api/catalog/products/quota")]
      [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "Supervisor", "SalesPerson")]
      public async Task<IEnumerable<ProductDto>> GetQuotaProductsCode()
      {
          return await _commandBus.SendAsync(new GetQuotaProductsQuery ());
      }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/disable")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup", "BuyerGroup")]
        public async Task<IActionResult> DisableProduct([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");

            return ApiCustomResponse(await _commandBus.SendAsync(new DisableProductCommand { Id = productId, CatalogId = id }));

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/enable")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup", "BuyerGroup")]
        public async Task<IActionResult> EnableProduct([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");

            return ApiCustomResponse(await _commandBus.SendAsync(new EnableProductCommand { Id = productId, CatalogId = id }));

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/hasQuota")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup", "BuyerGroup")]
        public async Task<IActionResult> ProductQuota([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");

            return ApiCustomResponse(await _commandBus.SendAsync(new ProductHasQuotaCommand { Id = productId, CatalogId = id }));

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/removeQuota")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup", "BuyerGroup")]
        public async Task<IActionResult> ProductRemoveQuota([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (productId == Guid.Empty)
                throw new InvalidOperationException("product id should not be null or empty");

            return ApiCustomResponse(await _commandBus.SendAsync(new ProductRemoveQuotaCommand { Id = productId, CatalogId = id }));

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/products/{productId:Guid}/isQuota")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer","SalesPerson")]
        public  Task<bool> ProductIsQuota([FromRoute] Guid productId)
        {
            return  _commandBus.SendAsync(new GetQuotaProductByIdQuery { ProductId = productId});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/products/{productId:Guid}/tax")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer","SalesPerson")]
        public  Task<decimal> ProductTax([FromRoute] Guid productId)
        {
            return  _commandBus.SendAsync(new GetTaxProductByIdQuery { ProductId = productId});
        }
        // [HttpPut]
        // [Consumes("application/json")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [Route("/api/catalog/{id:Guid}/products/{productId:Guid}/enable")]
        // [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup", "BuyerGroup")]
        // public async Task<IActionResult> EnableProduct([FromRoute] Guid id, [FromRoute] Guid productId)
        // {
        //     if (productId == Guid.Empty)
        //         throw new InvalidOperationException("product id should not be null or empty");
        //
        //     return ApiCustomResponse(await _commandBus.SendAsync(new EnableProductCommand { Id = productId, CatalogId = id }));
        //
        // }
    }
}
