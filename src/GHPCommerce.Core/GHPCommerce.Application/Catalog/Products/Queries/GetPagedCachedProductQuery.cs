using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Infra.Cache;
using Serilog.Core;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetPagedCachedProductQuery : ICommand<SyncPagedResult<ProductDtoV6>>
    {
        public Guid SalesPersonId { get; set; }
        public Guid SupplierOrganizationId { get; set; }

        public String Term { get; set; }
        public SyncDataGridQuery GridQuery { get; set; }
        public List<Guid> ClassIds { get; set; }

    }

    public class GetPagedCachedProductQueryHandler : ICommandHandler<GetPagedCachedProductQuery, SyncPagedResult<ProductDtoV6>>
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly MedIJKModel _model;
        private readonly Logger _logger;

        public GetPagedCachedProductQueryHandler(IRepository<Product, Guid> productRepository,
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization, MedIJKModel model, Logger logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _model = model;
            _logger = logger;

        }
        public async Task<SyncPagedResult<ProductDtoV6>> Handle(GetPagedCachedProductQuery request, CancellationToken cancellationToken)
        {
            // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            // var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery {OrganizationId = orgId.Value});
            // var supplier = await _commandBus.SendAsync(new GetByIdOfSupplierQuery {Id = customer.SupplierId});
            var orgId = request.SupplierOrganizationId;
            
            if (Guid.Empty == orgId)
                throw new InvalidOperationException("");

            var availablesProduct =
                await _commandBus.SendAsync(new GetAvailableProductIdQuery {OrganizationId = orgId});
            var result = _productRepository.Table
                .Include(x => x.Manufacturer)
                .Include(x => x.TaxGroup)
                .Where(x => x.ProductState == ProductState.Valid)
                .DynamicWhereQuery(request.GridQuery);
            if (!String.IsNullOrEmpty(request.Term))
                result = result.Where(c => c.FullName.Contains(request.Term) || c.Manufacturer.Name.Contains(request.Term));
            if (request.ClassIds != null && request.ClassIds.Count() > 0 && !request.ClassIds.Any(c => c == Guid.Empty) )
                result = result.Where(c => request.ClassIds.Contains(c.ProductClassId.Value) || c.ProductClassId == null);
            var data  = await result
               
                .Select(p => new ProductDtoV6
                {
                    FullName = p.FullName,
                    Id = p.Id,
                    Code = p.Code,
                    Manufacturer = p.Manufacturer != null ? p.Manufacturer.Name : "",
                    Tax = p.TaxGroup != null ? p.TaxGroup.TaxValue / 100 : 0,
                    Psychotropic = p.Psychotropic,
                    ProductClassId = p.ProductClassId,
                    INNCodeId = p.INNCodeId,
                })
                .ToListAsync(cancellationToken);
         
            var field = request.GridQuery.Where.Find(item => item.Predicates.Find(c => c.Field.Equals("quantity") )!= null);
            if (field != null)
            {
                var filterQuantity = field.Predicates.Find(c => c.Field == "quantity")?.Value;
                if(filterQuantity != null && filterQuantity.ToString() == "1")  data = data.Where(c => availablesProduct.Contains(c.Id)).ToList();
                else data = data.Where(c => !availablesProduct.Contains(c.Id)).ToList();
            }
            
            var total =  data.Count();
            data = data.AsQueryable().Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take).ToList();
            var availableProducts = await _commandBus.SendAsync(new GetAvailableQuantitiesInStockQuery
                { ProductIds = data.Select(x => x.Id).ToList(), SupplierId = orgId}, cancellationToken);
           
            foreach (var item in data.OrderBy(x => x.FullName))
            {
                if (availableProducts.ContainsKey(item.Id))
                {
                    item.Quantity = availableProducts[item.Id].Item1;
                    item.TotalQnt = availableProducts[item.Id].Item1;
                    item.SalePrice = availableProducts[item.Id].Item2;
                }
            }
            if (!_model.AXInterfacing)
            {
                var remainQuantities = (await _commandBus.SendAsync(new GetRQQuery { ProductIds = result.Select(x => x.Id).ToList() }, cancellationToken)).ToArray();
                if (remainQuantities.Any())
                {
                    foreach (var productDtoV3 in data.Where(productDtoV3 => remainQuantities.Any(p => p.ProductId == productDtoV3.Id)))
                    {
                        var p = remainQuantities.First(p => p.ProductId == productDtoV3.Id);
                        productDtoV3.TotalQnt = productDtoV3.Quantity +  p.QuotaQnt;
                        productDtoV3.TotalRQ = p.RemainQnt;
                    }
                } 
            }
            return new SyncPagedResult<ProductDtoV6> {Result = data, Count = total};
        }
    }

}