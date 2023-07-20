using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Application.Tiers.Customers.Queries;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class ProductQueriesHandler :
        ICommandHandler<GetProductListQuery , PagingResult<ProductDto>>,
        ICommandHandler<GetProductByIdQuery, ProductDtoV2>,
        ICommandHandler<GetListProductsByIds, IEnumerable<ProductDtoV3>>,
        ICommandHandler<GetProductById, ProductDtoV3>,
        ICommandHandler<GetProductByCode, ProductDtoV3>,
        ICommandHandler<ActiveProductExists, bool>,
        ICommandHandler<GetProductSharedQuery,IEnumerable<ProductDtoV3>>, 
        ICommandHandler<GetProductListForB2BCustomerQuery, PagingResult<ProductDtoV3>>,
        ICommandHandler<GetProductTaxAndInnCodeQuery,ProductDtoV4 >,
        ICommandHandler<GetProductIdsByInnCodeIdQuery, IEnumerable<Guid>>,
        ICommandHandler<GetListOfProductsByCatalogAndNameQuery, IEnumerable<ProductDtoV3>>,
        ICommandHandler<GetProductIdsByCatalogId, IEnumerable<ProductDtoV4>>,
        ICommandHandler<ActiveProductExistsByCode, bool>,
        ICommandHandler<GetProductListForSalesPersonQuery, IEnumerable<ProductDtoV5>>,
        ICommandHandler<GetPagedProductListQuery, SyncPagedResult<ProductDto>>,
        ICommandHandler<GetLastProductCodeUQuery, string>,
        ICommandHandler<GetQuotaProductsQuery, IEnumerable<ProductDto>>


    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly MedIJKModel _model;

        public ProductQueriesHandler(IRepository<Product, Guid> productRepository, 
            IMapper mapper, 
            ICommandBus commandBus, 
            ICurrentOrganization currentOrganization, MedIJKModel model)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _model = model;
        }
        public async Task<PagingResult<ProductDto>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
        {
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "FullName " + request.SortDir : GetSortQuery(request.SortProp, request.SortDir);

            var query = _productRepository.Table;
              
            if (!string.IsNullOrEmpty(request.Term))
            {
                query = query
                    .Where(x => x.FullName.ToLower().Contains(request.Term.ToLower())
                                || x.Code.ToLower().Contains(request.Term.ToUpper()) ||
                                x.Manufacturer.Name.ToLower().Contains(request.Term.ToLower())
                                || x.ProductClass.Name.ToLower().Contains(request.Term.ToLower()));
            }
            var total = await query.CountAsync(cancellationToken);
            query = query.Include(x => x.Manufacturer)
                .Include(x => x.ProductClass)
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize); var data = await  query
                .Select(x=> new ProductDto(x))
                .ToListAsync(cancellationToken);
            return new PagingResult<ProductDto> { Data = data, Total = total };
        }

        private string GetSortQuery(string requestSortProp, string requestSortDir)
        {
            // ReSharper disable once ComplexConditionExpression
            if (requestSortProp != "Manufacturer" && requestSortProp!= "ProductClass")
                return  requestSortProp+ " " + requestSortDir;
            if(requestSortProp == "Manufacturer")
                return "Manufacturer.Name " + requestSortDir;
            if (requestSortProp == "ProductClass")
                return "ProductClass.Name " + requestSortDir;
            return String.Empty;

        }

        public async Task<ProductDtoV2> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
           
            var existingProduct = await _productRepository
                .Table
                .Include(x=>x.List)
                .Include(x => x.Manufacturer)
                .Include(x => x.Brand)
                .Include(x => x.ProductClass)
                .Include(x => x.TaxGroup)
                .Include(x=>x.PickingZone)
                .ThenInclude(z=>z.ZoneGroup)
                .AsNoTracking()

                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
                // ReSharper disable once TooManyChainedReferences
                .ConfigureAwait(false);
            // ReSharper disable once ComplexConditionExpression
            if (existingProduct == null )
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            var rst = _mapper.Map<ProductDtoV2>(existingProduct);
            rst.Images = new List<string>();
            foreach (var item in existingProduct.Images)
            {
                rst.Images.Add(Convert.ToBase64String(item.ImageBytes));
            }
            rst.ProductClassName = existingProduct.ProductClass?.Name;
            rst.PharmacologicalClassName = existingProduct.PharmacologicalClass?.Name;
            rst.BrandName = existingProduct.Brand?.Name;
            rst.Manufacturer = existingProduct.Manufacturer?.Name;
            rst.PickingZone = existingProduct.PickingZone;
            rst.ZoneGroup = _mapper.Map<ZoneGroupDto>( existingProduct.PickingZone?.ZoneGroup);
            rst.PickingZone.ZoneGroup = null;
            rst.PickingZone.Products = null;
            rst.DefaultLocation = existingProduct.DefaultLocation;
            return rst;

        }
        public async Task<ProductDtoV3> Handle(GetProductById request, CancellationToken cancellationToken)
        {

            var existingProduct = await _productRepository
                .Table
                .Include(x => x.TaxGroup)
                .Select(x => new ProductDtoV3
                {
                    FullName = x.FullName, 
                    Code = x.Code, 
                    Id = x.Id, 
                    Tax = x.TaxGroup.TaxValue, 
                    HasQuota = x.Quota ,
                    Psychotropic = x.Psychotropic ,
                    Manufacturer = x.Manufacturer.Name,
                    INNCodeId = x.INNCodeId,
                    Brand = x.Brand.Name,
                    ProductClassName = x.ProductClass.Name,
                    ProductState = x.ProductState
                })
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken) 

                .ConfigureAwait(false);
            // ReSharper disable once ComplexConditionExpression
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            return existingProduct;

        }

        public async Task<IEnumerable<ProductDtoV3>> Handle(GetListProductsByIds request, CancellationToken cancellationToken)
        {
            if (request.Ids == null) return new List<ProductDtoV3>();
            var existingProduct = await _productRepository
                .Table
                .Select(x => new ProductDtoV3 {FullName = x.FullName, Code = x.Code, Id = x.Id})
                .Where(x => request.Ids.Any(p => p == x.Id))
                .ToListAsync(cancellationToken);
            return existingProduct;


        }

        public async Task<bool> Handle(ActiveProductExists request, CancellationToken cancellationToken)
        {
            return await _productRepository
                .Table
                .AnyAsync(x =>x.Id==request.Id && x.ProductState==ProductState.Valid, cancellationToken);

        }

        public async Task<IEnumerable<ProductDtoV3>> Handle(GetProductSharedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result= await _productRepository
                    .Table
                    .Where(x => x.ProductState == ProductState.Valid)
                    .Select(x => new ProductDtoV3 { FullName = x.FullName, Code = x.Code, Id = x.Id,PurchasePrice = x.PurchasePrice,SalePrice = x.SalePrice})
                    .OrderBy(p => p.FullName).ToListAsync(cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
            // ReSharper disable once ComplexConditionExpression
        }

        public async Task<PagingResult<ProductDtoV3>> Handle(GetProductListForB2BCustomerQuery request, CancellationToken cancellationToken)
        {
            var orgId =await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!orgId.HasValue)
                throw new InvalidOperationException("");
            var allowedProductClasses =await _commandBus.SendAsync(new GetAllowedProductClassesQuery {CustomerOrganizationId = orgId.Value, SupplierOrganizationId = request.SupplierId}, cancellationToken);
            var classes = allowedProductClasses.Select(x=>x.ProductClassId).ToArray();
            var query  = _productRepository.Table
                .Include(v => v.ProductClass)
                .Where(x => classes.Any(c => c == x.ProductClassId.Value) && x.ProductState == ProductState.Valid);
            if (!string.IsNullOrEmpty(request.Term))
                query = query.Where(x => x.FullName.ToLower().Contains(request.Term.ToLower())
                                       || x.Code.ToLower().Contains(request.Term.ToUpper()));
            if(request.ProductId != null && request.ProductId != Guid.Empty)
            {
                var product = await _commandBus.SendAsync(new GetProductTaxAndInnCodeQuery { ProductId = (Guid)request.ProductId }, cancellationToken);
                query = query.Where(x => x.INNCodeId == product.InnCodeId.Value);
            }
            var total = await query.CountAsync(cancellationToken: cancellationToken);
           var products =  await query
               .Select(p => new ProductDtoV3 { FullName = p.FullName, Id = p.Id, ProductClassName = p.ProductClass.Name, INNCodeId= p.INNCodeId ,})
                .OrderBy(prop => prop.FullName)
                .Paged(request.page,request.pageSize)
                .ToListAsync(cancellationToken);
            var availableProducts = await _commandBus.SendAsync(new GetAvailabilityInStockQuery
            { ProductIds = products.Select(x => x.Id).ToList() }, cancellationToken);
            foreach (var productDtoV3 in products)
                productDtoV3.Available = availableProducts.TryGetValue(productDtoV3.Id, out var stockExistence) &&
                                         stockExistence;

            return new PagingResult<ProductDtoV3> { Total = total, Data = products };

        }

        public async Task<ProductDtoV4> Handle(GetProductTaxAndInnCodeQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository
                .Table
                .Include(x => x.TaxGroup)
                .Where(x => x.Id == request.ProductId)
                .Select(x => new ProductDtoV4 {Id = x.Id, TaxGroup = x.TaxGroup.TaxValue, InnCodeId = x.INNCodeId, UnitPrice = x.SalePrice})
                .FirstOrDefaultAsync(cancellationToken);
            return product;
        }

        public async Task<IEnumerable<Guid>> Handle(GetProductIdsByInnCodeIdQuery request, CancellationToken cancellationToken)
        {

           var productIds =  await _productRepository
                .Table
                .Where(x => x.INNCodeId.Value == request.InnCodeId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
#if DEBUG
            foreach (var productId in productIds)
            {
                Console.WriteLine(productId.ToString());
            }
#endif
            return productIds;


        }

        public async Task<IEnumerable<ProductDtoV3>> Handle(GetListOfProductsByCatalogAndNameQuery request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.SearchName) || string.IsNullOrWhiteSpace(request.SearchName))
                return new List<ProductDtoV3>();
            var query = _productRepository.Table
                .Where(x => x.FullName.ToLower().Contains(request.SearchName.ToLower()));
            if (request.CatalogId!= Guid.Empty)
                query = query.Where(x => x.ProductClassId == request.CatalogId);
               
            return await query.Select(p => new ProductDtoV3 { FullName = p.FullName, Id = p.Id, ProductClassName = p.ProductClass.Name, CatalogId = p.ProductClassId.Value , HasQuota = p.Quota})
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductDtoV4>> Handle(GetProductIdsByCatalogId request, CancellationToken cancellationToken)
        {
            return await _productRepository.Table
                .Include(x=>x.Manufacturer)
                .Include(x=>x.Brand)
                .Include(x=>x.TaxGroup)
                .Where(x => x.ProductClassId == request.CatalogId && x.ProductState == ProductState.Valid)
                .OrderBy(x=>x.FullName)
                .Select(x =>  new ProductDtoV4 {Id=x.Id, TaxGroup = x.TaxGroup.TaxValue, Brand = x.Brand.Name, BrandId = x.BrandId, ManufacturerId  =  x.ManufacturerId.Value,Manufacturer = x.Manufacturer.Name, FullDescription = x.Description,Images = x.Images.Select(i=>Convert.ToBase64String(i.ImageBytes) ).ToList()})
                .ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task<ProductDtoV3> Handle(GetProductByCode request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
                .Table
                .Include(x => x.TaxGroup)
                .Include(x=>x.PickingZone)
                .Include(x=>x.PickingZone.ZoneGroup)
                .Select(x => new ProductDtoV3
                {
                    FullName = x.FullName, 
                    Code = x.Code, 
                    Id = x.Id, 
                    Tax = x.TaxGroup != null ? x.TaxGroup.TaxValue / 100 : 0,
                    TaxGroupCode = x.TaxGroup.Code,
                    ProductState=x.ProductState,
                    DefaultLocation = x.DefaultLocation, 
                    PickingZoneId = x.PickingZoneId,
                    ZoneGroupId = x.PickingZone.ZoneGroupId,
                    PickingZoneName = x.PickingZone.Name,
                    ZoneGroupName = x.PickingZone.ZoneGroup.Name,
                    PickingZoneOrder = x.PickingZone.Order,
                    HasQuota = x.Quota
                    
                })
                .FirstOrDefaultAsync(x => x.Code == request.CodeProduct, cancellationToken);
            // ReSharper disable once ComplexConditionExpression
            return existingProduct;
        }

        public async Task<bool> Handle(ActiveProductExistsByCode request, CancellationToken cancellationToken)
        {
            return await _productRepository
                .Table
                .AnyAsync(x => x.Code == request.Code && x.ProductState == ProductState.Valid, cancellationToken);

        }

        public async Task<IEnumerable<ProductDtoV5>> Handle(GetProductListForSalesPersonQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!orgId.HasValue)
                throw new InvalidOperationException("");
            var query =  _productRepository.Table
                .Include(x => x.Manufacturer)
                .Include(x => x.InnCode)
                .Include(x => x.ProductClass)
                .Include(x => x.TaxGroup)
                .Include(x => x.List)
                .Where(x=>x.ProductState == ProductState.Valid 
                          &&( string.IsNullOrEmpty(request.SearchBy) || x.FullName.ToLower().StartsWith(request.SearchBy.ToLower()) || x.Code.Contains(request.SearchBy) || x.InnCode!=null 
                              && x.InnCode.Name .StartsWith(request.SearchBy) 
                              &&( request.IsPsy.HasValue && x.Psychotropic == request.IsPsy || !request.IsPsy.HasValue) )
                         );
                if (request.IsPsy.HasValue)
                query = query.Where(x =>  x.Psychotropic == request.IsPsy);
                
                var result = await query.Select(p => new ProductDtoV5
                {
                    FullName = p.FullName,
                    Id = p.Id,
                    Code = p.Code,
                    Manufacturer =p.Manufacturer!=null? p.Manufacturer.Name : "",
                    TaxGroup = p.TaxGroup != null ? p.TaxGroup.Name : "",
                    Tax = p.TaxGroup != null ? p.TaxGroup.TaxValue / 100 : 0,
                    Psychotropic = p.Psychotropic,
                    ProductClassName = p.ProductClass != null ? p.ProductClass.Name : "",
                    INNCodeId = p.INNCodeId,
                    INNCodeName = p.InnCode != null ? p.InnCode.Name : "",
                    HasQuota = p.Quota,
                    PickingZoneId = p.PickingZoneId,
                    Thermolabile = p.Thermolabile,
                    DefaultLocation = p.DefaultLocation,
                    PFS =p.List!=null? p.List.SHP : 0,
                    DciFullName = p.DciConcat
                    
                }).ToListAsync(cancellationToken);
            var availableProducts = await _commandBus.SendAsync(new GetAvailableQuantitiesInStockQuery
                { ProductIds = result.Select(x => x.Id).ToList(), SupplierId = orgId }, cancellationToken);
            foreach (var productDtoV3 in result.OrderBy(x => x.FullName))
            {
                if (availableProducts.ContainsKey(productDtoV3.Id))
                {
                    productDtoV3.Quantity = availableProducts[productDtoV3.Id].Item1;
                    productDtoV3.TotalQnt = availableProducts[productDtoV3.Id].Item1;
                    productDtoV3.SalePrice = availableProducts[productDtoV3.Id].Item2;
                }
                
            }

            if (!_model.AXInterfacing)
            {
                var remainQuantities = (await _commandBus.SendAsync(new GetRQQuery { ProductIds = result.Select(x => x.Id).ToList() }, cancellationToken)).ToArray();
                if (remainQuantities.Any())
                {
                    foreach (var productDtoV3 in result.Where(productDtoV3 => remainQuantities.Any(p => p.ProductId == productDtoV3.Id)))
                    {
                        var p = remainQuantities.First(p => p.ProductId == productDtoV3.Id);
                        productDtoV3.TotalQnt = productDtoV3.Quantity +  p.QuotaQnt;
                        productDtoV3.TotalRQ = p.RemainQnt;
                    }
                } 
            }
            return result.OrderBy(x => x.FullName);
        }

        public async Task<SyncPagedResult<ProductDto>> Handle(GetPagedProductListQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository.Table;
            if (request.GridQuery.Where != null /*&& request.GridQuery.Where.Predicates.Any()*/)
            {
                foreach (var wherePredicate in request.GridQuery.Where[0].Predicates)
                {
                    if (wherePredicate.Field == "manufacturerName")
                        query = query .Where(p=> p .Manufacturer.Name.Contains(wherePredicate.Value.ToString()));
                    else if (wherePredicate.Field == "productClassName")
                        query = query .Where(p=> p .ProductClass.Name.Contains(wherePredicate.Value.ToString()));
                }
            }
            if (request.GridQuery.Sorted!=null&& request.GridQuery.Sorted.Any())
            {
                foreach (var sorted in request.GridQuery.Sorted)
                {
                    if (sorted.Name == "manufacturerName")
                    {
                        query = (sorted.Direction == "ascending")
                            ? query.OrderBy(x => x.Manufacturer.Name)
                            : query.OrderByDescending(x => x.Manufacturer.Name);
                        request.GridQuery.Sorted = null;
                    }
                    if (sorted.Name == "productClassName")
                    {
                        query = (sorted.Direction == "ascending")
                            ? query.OrderBy(x => x.ProductClass.Name)
                            : query.OrderByDescending(x => x.ProductClass.Name);
                        request.GridQuery.Sorted = null;
                    }
                  
                }

               
            }

            query = query.DynamicWhereQuery(request.GridQuery);
            var total = await query
                .Include(x => x.Manufacturer)
                .Include(x => x.ProductClass)
                .CountAsync(cancellationToken);
            query = query
                .Include(x => x.Manufacturer)
                .Include(x => x.ProductClass)
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take);
            var data = await query
                .Select(x => new ProductDto(x))
                .ToListAsync(cancellationToken);
            return new SyncPagedResult<ProductDto> {Result = data, Count = total};
        }

        public async Task<string> Handle(GetLastProductCodeUQuery request, CancellationToken cancellationToken)
        {
            var parsedInt = await _productRepository.Table
                .Where(x => !string.IsNullOrEmpty(x.Code) && !string.IsNullOrWhiteSpace(x.Code))
                .OrderByDescending(x => x.Code)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken);
               
            return ParseStringContainingNumber(parsedInt).ToString();
        }
        private static int? ParseStringContainingNumber(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return null;
            }

            var numbersInInput = new String(input.Where(Char.IsDigit).ToArray());
            if (String.IsNullOrEmpty(numbersInInput))
            {
                return null;
            }

            int output;

            if (!Int32.TryParse(numbersInInput, out output))
            {
                return null;
            }

            return output + 1;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetQuotaProductsQuery request, CancellationToken cancellationToken)
        {
            var query =await _productRepository.Table
                .AsNoTracking()
                .Where(x=>x.Quota)
                .Select(p => new ProductDto(p))
                .ToListAsync(cancellationToken: cancellationToken);
            return query;

        }
    }
}
