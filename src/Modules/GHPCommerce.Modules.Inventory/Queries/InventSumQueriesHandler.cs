using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
namespace GHPCommerce.Modules.Inventory.Queries
{
    public class InventSumQueriesHandler :
        
        ICommandHandler<GetInventSumListQuery, PagingResult<InventSumDto>>,
        ICommandHandler<InventoryDimensionExistsQuery,bool>,
        ICommandHandler<InventoryDimensionExistsQueryV2, bool>,
        ICommandHandler<GetInventSumByIdQuery,InventSumDto>,
        ICommandHandler<GetInventSumByDimensionQuery,InventSumDto>,
        ICommandHandler<GetStockForB2BCustomerQuery, IEnumerable<InventSumDtoV1>>,
        ICommandHandler<GetStockByProductQuery, List<InventSumDtoV1>>,
        ICommandHandler<GetStockForSalesPerson, List<CachedInventSum>>,
        ICommandHandler<GetAvailabilityInStockQuery, Dictionary<Guid, bool>>,
        ICommandHandler<GetListOfProductsByCatalogQuery, PagingResult<ProductDtoV3>>,
        ICommandHandler<GetAvailableQuantitiesInStockQuery, Dictionary<Guid, Tuple<int, decimal>>>,
        ICommandHandler<GetPagedInventQuery, SyncPagedResult<InventSumDto>>,
        ICommandHandler<GetListOfProductForQuotaQuery, IEnumerable<InventSumQuotaDto>>,
        ICommandHandler<GetStockForPreparation, List<InventSumDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<InventSum, Guid> _inventSumRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICache _redisCache;
        private readonly MedIJKModel _model;
        private readonly PreparationInventEndPoint _preparationInventEndPoint;

        public InventSumQueriesHandler(IMapper mapper,
            IRepository<InventSum, Guid> inventSumRepository,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICache redisCache, MedIJKModel model, PreparationInventEndPoint preparationInventEndPoint)
        {
            _mapper = mapper;
            _inventSumRepository = inventSumRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _redisCache = redisCache;
            _model = model;
            _preparationInventEndPoint = preparationInventEndPoint;
        }

        public async Task<PagingResult<InventSumDto>> Handle(GetInventSumListQuery request,
            CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return null;
            var total = await _inventSumRepository.Table.CountAsync(x => x.OrganizationId == org &&
                                                                    ((string.IsNullOrEmpty(request.ProductFullName) ||
                                                                      x.ProductFullName.ToLower().Contains(request.ProductFullName.ToLower())) && (string.IsNullOrEmpty(request.ProductCode) ||
                                                                         (x.ProductCode.ToLower().Contains(request.ProductCode.ToLower()))) && (string.IsNullOrEmpty(request.VendorBatchNumber) ||
                                                                         (x.VendorBatchNumber.ToLower().Contains(request.VendorBatchNumber.ToLower())))
                                                                     && (string.IsNullOrEmpty(request.InternalBatchNumber) || (x.InternalBatchNumber.ToLower()
                                                                         .Contains(request.InternalBatchNumber.ToLower())))
                                                                     && (!request.IsPublic.HasValue || (x.IsPublic == request.IsPublic.Value))
                                                                     && (!request.ProductId.HasValue || (x.ProductId == request.ProductId.Value))), cancellationToken: cancellationToken);
                ;
            // ReSharper disable once ComplexConditionExpression
            string orderQuery = string.IsNullOrEmpty(request.SortProp)
                ? "ProductFullName " + request.SortDir
                : request.SortProp + " " + request.SortDir;

            var query = _inventSumRepository
                .Table
                .Where(x => x.OrganizationId == org &&
                      ((string.IsNullOrEmpty(request.ProductFullName) ||
                 x.ProductFullName.ToLower().Contains(request.ProductFullName.ToLower())) && (string.IsNullOrEmpty(request.ProductCode) ||
                    (x.ProductCode.ToLower().Contains(request.ProductCode.ToLower()))) && (string.IsNullOrEmpty(request.VendorBatchNumber) ||
                    (x.VendorBatchNumber.ToLower().Contains(request.VendorBatchNumber.ToLower())))
                && (string.IsNullOrEmpty(request.InternalBatchNumber) || (x.InternalBatchNumber.ToLower()
                    .Contains(request.InternalBatchNumber.ToLower())))
                && (!request.IsPublic.HasValue || (x.IsPublic == request.IsPublic.Value))
                && (!request.ProductId.HasValue || (x.ProductId == request.ProductId.Value))))
                .OrderBy(orderQuery)
                // ReSharper disable once TooManyChainedReferences
                .Paged(request.Page, request.PageSize);
            var _list = await query.ToListAsync(cancellationToken);
            for (int k = 0; k < _list.Count; k++)
            {
                var temp = await _redisCache.GetAsync<InventSumCreatedEvent>(_list[k].ProductId.ToString() + org, cancellationToken);
                var item = temp?.CachedInventSumCollection.CachedInventSums.FirstOrDefault(t => t.ProductId == _list[k].ProductId && t.InternalBatchNumber == _list[k].InternalBatchNumber && t.IsPublic == _list[k].IsPublic);
                if (item != null)
                    _list[k].PhysicalReservedQuantity = item.PhysicalReservedQuantity;
            } 
            var data = _mapper.Map<IEnumerable<InventSumDto>>( _list );
            
            return new PagingResult<InventSumDto> {Data = data, Total = total};

        }

        public async Task<bool> Handle(InventoryDimensionExistsQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return false;
            bool exists=
                await _inventSumRepository.Table.AnyAsync(x =>
                    x.OrganizationId == org &&
                    x.ProductId == request.ProductId &&
                    x.SiteId == request.SiteId &&
                    x.WarehouseId == request.WarehouseId &&
                    x.Color == request.Color &&
                    x.Size == request.Size &&
                    x.InternalBatchNumber == request.InternalBatchNumber &&
                    x.VendorBatchNumber == request.VendorBatchNumber &&
                    x.IsPublic == request.IsPublic 
                    , cancellationToken);
            return !exists;
        }

        public async Task<InventSumDto> Handle(GetInventSumByIdQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new InventSumDto();
            var inventSumDto = _mapper.Map<InventSumDto>
            (
                await _inventSumRepository.Table.FirstOrDefaultAsync(x =>
                        x.Id == request.Id
                    , cancellationToken));
            return inventSumDto;
        }

        public async Task<InventSumDto> Handle(GetInventSumByDimensionQuery request, CancellationToken cancellationToken)
        {
            
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (request.Dimension.OrganizationId.HasValue) org = request.Dimension.OrganizationId.Value;
                if (!org.HasValue)
                return new InventSumDto();
            var inventKey = request.Dimension.ProductId.ToString() + request.Dimension.OrganizationId;
            var invent = await _redisCache.GetAsync<InventSumCreatedEvent>(inventKey, cancellationToken);
            if (invent == null) return null;
            var inventSumDto = _mapper.Map<InventSumDto>
            (
                invent.CachedInventSumCollection.CachedInventSums.FirstOrDefault(x =>
                        x.OrganizationId == org &&
                        x.ProductId == request.Dimension.ProductId &&
                        x.SiteId == request.Dimension.SiteId &&
                        x.WarehouseId == request.Dimension.WarehouseId &&
                        x.Color == request.Dimension.Color &&
                        x.Size == request.Dimension.Size &&
                        x.InternalBatchNumber == request.Dimension.InternalBatchNumber &&
                        x.VendorBatchNumber == request.Dimension.VendorBatchNumber &&
                        x.IsPublic == request.Dimension.IsPublic
                    ));
            return inventSumDto;
        }

        public async Task<Dictionary<Guid, bool>> Handle(GetAvailabilityInStockQuery request, CancellationToken cancellationToken)
        {
            var ids = request.ProductIds.AsEnumerable();
            var items = await _inventSumRepository
                .Table
                .Where(x => !request.supplierId.HasValue || x.OrganizationId == request.supplierId.Value)
                .Where(x => ids.Any(p => p == x.ProductId))
                .Where(x => x.IsPublic &&
                            (x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now || x.ExpiryDate == null))
                .Select(x => new {x.ProductId, x.PhysicalAvailableQuantity, x.IsPublic})
                .ToListAsync(cancellationToken);
            var group = items
                .Where(x => x.PhysicalAvailableQuantity != 0)
                .GroupBy(x => x.ProductId);
            var dic = group.ToDictionary(item => item.Key, item => item.Sum(x => x.PhysicalAvailableQuantity) > 0);

            return dic;

        }

        public async Task<IEnumerable<InventSumDtoV1>> Handle(GetStockForB2BCustomerQuery request,CancellationToken cancellationToken)
        {
            var query = await GetInventSumsByProductId(request.SupplierId, request.ProductId, cancellationToken);
            var product = await _commandBus.SendAsync(new GetProductTaxAndInnCodeQuery {ProductId = request.ProductId,}, cancellationToken);
            var sum = query.FirstOrDefault();
            var  availableProducts  =  new List<InventSumDtoV1>();
            if (sum != null)
            {
                availableProducts.Add(SetInventSumDto(sum, query, product));
                return availableProducts;
            }
            if (product.InnCodeId == null) return availableProducts;

            var productIds = (await _commandBus.SendAsync(new GetProductIdsByInnCodeIdQuery{InnCodeId = product.InnCodeId.Value}, cancellationToken)).AsEnumerable();
            var equivalentProducts = await GetEquivalentProductsFromInventSum(request,cancellationToken, productIds);
            var itemsGroupedByProductId = GetInventSumsGroupedByProduct( equivalentProducts);
            var inventSums = itemsGroupedByProductId as IGrouping<Guid, InventSum>[] ?? itemsGroupedByProductId.ToArray();
            foreach (var item in inventSums)
            {
                var firstItem = item.FirstOrDefault();
                if (firstItem == null) continue;

                var p = await _commandBus.SendAsync(new GetProductTaxAndInnCodeQuery {ProductId = firstItem.ProductId}, cancellationToken);

                var inventSum = SetInventSumDto(firstItem, item.ToList(), p);
                availableProducts.Add(inventSum);
            }

            return availableProducts;
        }

        private static IEnumerable<IGrouping<Guid, InventSum>> GetInventSumsGroupedByProduct( List<InventSum> equivalentProducts)
        {
            var items = equivalentProducts
                .Where(x=>x.PhysicalAvailableQuantity!=0)
                .OrderBy(x => x.ExpiryDate)
                .GroupBy(x => x.ProductId);
            return items;
        }

        private async Task<List<InventSum>> GetEquivalentProductsFromInventSum(GetStockForB2BCustomerQuery request, CancellationToken cancellationToken, IEnumerable<Guid> productIds)
        {
            var equivalentProducts = await _inventSumRepository
                .Table
                .Where(x => productIds.Any(p => p == x.ProductId))
                .Where(x => x.OrganizationId == request.SupplierId
                            && (x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now.Date || x.ExpiryDate == null)
                            && x.IsPublic)

                .ToListAsync(cancellationToken);
            return equivalentProducts;
        }

        private async Task<List<InventSum>> GetInventSumsByProductId(Guid supplierId,Guid productId,
            CancellationToken cancellationToken)
        {
            var query = await _inventSumRepository
                .Table
                .Where(x => x.OrganizationId == supplierId
                            && x.ProductId == productId
                            && (x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now.Date || x.ExpiryDate == null)
                            && x.IsPublic )
                .OrderBy(x => x.ExpiryDate)
                .ThenBy(x => x.ProductFullName)
                .ToListAsync(cancellationToken);
            return query.Where(x => x.PhysicalAvailableQuantity != 0).ToList();
        }

        private static InventSumDtoV1 SetInventSumDto(InventSum sum, List<InventSum> query, ProductDtoV4 product)
        {
            var inventSum = new InventSumDtoV1();
            inventSum.BestBeforeDate = sum.BestBeforeDate;
            inventSum.Color = sum.Color;
            inventSum.ProductCode = sum.ProductCode;
            inventSum.ProductFullName = sum.ProductFullName;
            inventSum.ProductId = sum.ProductId;
            inventSum.ExpiryDate = sum.ExpiryDate;
            inventSum.TotalPhysicalAvailableQuantity = query.Sum(x => x.PhysicalAvailableQuantity);
            inventSum.SalesDiscountRatio = sum.SalesDiscountRatio;
            inventSum.SalesUnitPrice = product.UnitPrice;
            inventSum.Tax = Convert.ToDouble(product.TaxGroup);
            inventSum.PackagingCode = sum.PackagingCode;
            return inventSum;
        }

        public async Task<PagingResult<ProductDtoV3>> Handle(GetListOfProductsByCatalogQuery request, CancellationToken cancellationToken)
        {
            var cachedProducts = await _redisCache.GetAsync<IEnumerable<ProductDtoV3>>(request.CatalogId + "_products", cancellationToken);
            if (cachedProducts != null&&cachedProducts.Any())
            {
                var result = cachedProducts
                    .DistinctBy(x => new { x.ProductId, x.UnitPrice, x.Tax, x.Discount , x.OrganizationId})
                    .ToList()
                    .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                    .OrderBy(x => x.FullName);
                return new PagingResult<ProductDtoV3> { Total = cachedProducts.DistinctBy(x => new { x.ProductId, x.UnitPrice, x.Tax, x.Discount, x.OrganizationId }).Count(), Data = result };
            }
            var productIds = (await GetProductIds(request, cancellationToken)).ToArray();
            var ids = productIds.Select(x => x.Id)
                .Distinct()
                .ToArray();
            var orgIds = await GetOrgIds(cancellationToken);
          
           
            var query = await (from q in _inventSumRepository.Table
                where ids.Any(p => p == q.ProductId) 
                      && orgIds.Any(o => o == q.OrganizationId)
                      && (q.ExpiryDate.HasValue && q.ExpiryDate > DateTime.Now.Date ||q.ExpiryDate == null)
                      && q.IsPublic
                      orderby q.ProductFullName
                    select new 
                {
                    
                    FullName = q.ProductFullName,
                    Code = q.ProductCode,
                    q.Id,
                    q.ProductId,
                    Available = q.PhysicalOnhandQuantity > q.PhysicalReservedQuantity,
                    Discount = q.SalesDiscountRatio ?? 0,
                    q.OrganizationId,
                    UnitPrice = Convert.ToDecimal(q.SalesUnitPrice)

                })
                .ToListAsync(cancellationToken);
            var productDtoV3S = query
                .Select(x => new ProductDtoV3
            {
                CatalogId = request.CatalogId,
                FullName = x.FullName,
                Code = x.Code,
                Id = x.Id,
                ProductId = x.ProductId,
                Available = x.Available,
                Discount = x.Discount,
                OrganizationId = x.OrganizationId,
                UnitPrice = Convert.ToDecimal(x.UnitPrice),
              

            });
            var productDtoV5S = productDtoV3S as ProductDtoV3[] ?? productDtoV3S.ToArray();
            foreach (var productDtoV3 in productDtoV5S)
            {
                var item = productIds.FirstOrDefault(x => x.Id == productDtoV3.ProductId);
                if (item == null) continue;
                productDtoV3.Tax = item.TaxGroup;
                productDtoV3.FullDescription = item.FullDescription;
                productDtoV3.Manufacturer = item.Manufacturer;
                productDtoV3.Brand = item.Brand;
                productDtoV3.BrandId = item.BrandId;
                productDtoV3.ManufacturerId = item.ManufacturerId;

            }
            await _redisCache.AddOrUpdateAsync<IEnumerable<ProductDtoV3>>(request.CatalogId + "_products", productDtoV5S.ToList(),cancellationToken);
            var data = productDtoV5S
                .ToList()
                .DistinctBy(x => new {x.ProductId, x.UnitPrice, x.Tax, x.Discount, x.OrganizationId})
                .ToList()
                //.Paged(request.Page, request.PageSize)
                .OrderBy(x => x.FullName);
           
            return new PagingResult<ProductDtoV3> { Total = data.Count(), Data = data.ToList().Skip((request.Page - 1) * request.PageSize).Take(request.PageSize) };
        }

        private async Task<Guid[]> GetOrgIds(CancellationToken cancellationToken)
        {
            var orgIds = await _redisCache.GetAsync<IEnumerable<Guid>>("_e-commerce_org", cancellationToken);
            if (orgIds !=null&&orgIds.Any()) return orgIds.ToArray();
            orgIds = (await _commandBus.SendAsync(new GetECommerceOrganizationIdsQuery(), cancellationToken)).ToArray();
            await _redisCache.AddOrUpdateAsync<IEnumerable<Guid>>("_e-commerce_org", orgIds, cancellationToken);

            return orgIds.ToArray();
        }

        private async Task<IEnumerable<ProductDtoV4>> GetProductIds(GetListOfProductsByCatalogQuery request, CancellationToken cancellationToken)
        {
            var productIds = await _redisCache.GetAsync<IEnumerable<ProductDtoV4>>(request.CatalogId.ToString(), cancellationToken);

            if (productIds!=null &&productIds.Any()) return productIds;
            productIds =
                (await _commandBus.SendAsync(new GetProductIdsByCatalogId {CatalogId = request.CatalogId},
                    cancellationToken)).ToArray();
            await _redisCache.AddOrUpdateAsync<IEnumerable<ProductDtoV4>>(request.CatalogId.ToString(), productIds.ToList(), cancellationToken);

            return productIds;
        }

        public async Task<bool> Handle(InventoryDimensionExistsQueryV2 request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();

            bool exists =
                await _inventSumRepository.Table.AnyAsync(x =>
                        x.OrganizationId == org &&
                        x.ProductCode == request.ProductCode &&
                        x.SiteId == request.SiteId &&
                        x.WarehouseId == request.WarehouseId &&
                        x.Color == request.Color &&
                        x.Size == request.Size &&
                        x.InternalBatchNumber == request.InternalBatchNumber &&
                        x.VendorBatchNumber == request.VendorBatchNumber &&
                        x.IsPublic == request.IsPublic
                    , cancellationToken);
            return !exists;
        }

        public async Task<List<CachedInventSum>> Handle(GetStockForSalesPerson request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue) return new  List<CachedInventSum>();
            var inventKey = request.ProductId.ToString() + org.Value;
            var result =  await _redisCache.GetAsync<InventSumCreatedEvent>(inventKey, cancellationToken);
            return result?.CachedInventSumCollection.CachedInventSums
                .Where(x => x.IsPublic &&x.OrganizationId==org.Value&&
                (x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now ||
                 x.ExpiryDate == null) && x.PhysicalAvailableQuantity>0)
                .OrderBy(x=>x.ExpiryDate)
                .ToList();

        }

        public async Task<Dictionary<Guid, Tuple<int, decimal>>> Handle(GetAvailableQuantitiesInStockQuery request, CancellationToken cancellationToken)
        {
           
            var ids = request.ProductIds.Select(x => x.ToString()).ToList();
            var invents = await _redisCache.GetAsync<InventSumCreatedEvent>(ids.Select(key =>  key.ToString() + request.SupplierId ).ToArray(), cancellationToken);
            var cachedInvents = invents.Select(x => x.CachedInventSumCollection).SelectMany(x => x.CachedInventSums);
            var items = cachedInvents.Where(x => x.IsPublic &&
                                                 (x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now ||
                                                  x.ExpiryDate == null))
                .Select(x => new {x.ProductId, x.PhysicalAvailableQuantity, x.IsPublic, x.SalesUnitPrice});
              
            var group = items
                //.Where(x => x.PhysicalAvailableQuantity != 0)
                .GroupBy(x => x.ProductId);
            var dic = group.ToDictionary(item => item.Key, item => new Tuple<int, decimal>( item.Sum(x => (int)x.PhysicalAvailableQuantity),(decimal)(item.Sum(x => x.SalesUnitPrice.Value) /item.Count())  ));

            return dic;
        }

        public async Task<SyncPagedResult<InventSumDto>> Handle(GetPagedInventQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return null;
          
            var query = _inventSumRepository.Table
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            
            var total = await query.CountAsync(cancellationToken: cancellationToken);

            var result = await query
             // .OrderByDescending(x => x.CreatedDateTime)
              .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1, request.SyncDataGridQuery.Take)
              .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<InventSumDto>>(result);
           
            return new SyncPagedResult<InventSumDto>
            { Result = data, Count = total };
        }
        public async Task<List<InventSumDto>> Handle(GetStockForPreparation request, CancellationToken cancellationToken)
        {
            var res=(await ExecuteRestRequestAsync<List<InventSumDto>>(new
            {
                ProductCode = request.ProductCode,
                ZoneName = request.ZoneName
            }, Method.POST, _preparationInventEndPoint.URL));
            return JsonConvert.DeserializeObject<List<InventSumDto>>(res.Content);
        }
        private static async Task<IRestResponse<T>> ExecuteRestRequestAsync<T>(object body, Method method, string url)
        {
            RestClient restClient = new RestClient(url);
            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/json; charset=utf-8 ");
            if (method != Method.GET)
            {
                request.RequestFormat = DataFormat.Json;
                request.Method = method;
                request.AddJsonBody(body);
            }

            var rsp = await restClient.ExecuteAsync<T>(request);
            return rsp;
        }

        public async Task<IEnumerable<InventSumQuotaDto>> Handle(GetListOfProductForQuotaQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (org == null)
                return  new List<InventSumQuotaDto>();
          
            List<InventSumQuotaDto> result;
            if (!_model.AXInterfacing)
            {
                var query = await _inventSumRepository
                    .Table
                    .Where(x => x.OrganizationId == org && x.PhysicalDispenseQuantity >= 0)
                    .Select(x => new QuotaDto
                    {
                        Id = x.ProductId,
                        InternalBatchNumber= x.InternalBatchNumber,
                        ProductCode= x.ProductCode,
                        ProductFullName = x.ProductFullName,
                        PhysicalDispenseQuantity = x.PhysicalDispenseQuantity

                    }).ToListAsync(cancellationToken);
                result = await OrderQueryByProduct(query);

            }
            else
            {
                var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery(), cancellationToken);
                var productIds = quotaProducts.Select(x => x.Id);
                var query = await _inventSumRepository
                    .Table
                    .Where(x => x.OrganizationId == org
                                && x.IsPublic  && productIds.Any(p=> p == x.ProductId))
                    .Select(x => new QuotaDto
                    {
                        Id = x.ProductId,
                        InternalBatchNumber= x.InternalBatchNumber,
                        ProductCode= x.ProductCode,
                        ProductFullName = x.ProductFullName,
                        PhysicalDispenseQuantity = x.PhysicalOnhandQuantity - x.PhysicalReservedQuantity

                    }).ToListAsync(cancellationToken);
                result = await OrderQueryByProduct(query);
                
            }
            return result;

        }

        private async Task< List<InventSumQuotaDto>> OrderQueryByProduct(List<QuotaDto> query)
        {
            var result = new List<InventSumQuotaDto>();
            foreach (var items in query.GroupBy(x => x.ProductCode))
            {
                foreach (var item in items)
                {
                    if (result.All(x => x.ProductCode != item.ProductCode))
                    {
                        result.Add(new InventSumQuotaDto
                        {
                            ProductCode = item.ProductCode,
                            ProductFullName = item.ProductFullName,
                            PhysicalDispenseQuantity = item.PhysicalDispenseQuantity,
                            ProductId = item.Id
                        });
                    }
                    else
                    {
                        var index = result.FindIndex(x => x.ProductCode == item.ProductCode);
                        if (index > -1)
                            result[index].PhysicalDispenseQuantity += item.PhysicalDispenseQuantity;
                    }
                }
            }

            foreach (InventSumQuotaDto inventSumQuotaDto in result)
            {
                var q = await _commandBus.SendAsync(new GetQuotasByProductQueryV2 { ProductId = inventSumQuotaDto.ProductId });
                if (q > 0)
                {
#if DEBUG
                    if(inventSumQuotaDto.ProductId == Guid.Parse("4E60BEAB-6583-EC11-B907-00155D001E06"))
                        Console.WriteLine(query);
#endif
                    inventSumQuotaDto.PhysicalDispenseQuantity -= q;
                }
            }

            return result;
        }

        public async Task<List<InventSumDtoV1>> Handle(GetStockByProductQuery request, CancellationToken cancellationToken)
        {
            List<InventSumDtoV1> results = new List<InventSumDtoV1>();
            foreach (var c in request.ProductCodes)
            {
                var prod = await _commandBus.SendAsync(new GetProductByCode() { CodeProduct = c }, cancellationToken);

                if (prod != null && prod.ProductState == Domain.Domain.Catalog.ProductState.Valid)
                {
                    var result = new InventSumDtoV1()
                    {
                        ProductId = prod.Id,
                        ProductCode = c,
                        TotalPhysicalAvailableQuantity = 0
                    };
                    var query = await GetInventSumsByProductId(request.SupplierId, prod.Id, cancellationToken);
                    if (query != null)
                        result.TotalPhysicalAvailableQuantity = query.Sum(p => p.PhysicalAvailableQuantity);
                    results.Add(result);
                }
            }


            return results;
 
        }
    }

    internal class QuotaDto
    {
        public Guid Id { get; set; }
        public string InternalBatchNumber { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public double? PhysicalDispenseQuantity { get; set; }
    }
}
