using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetEquivalencesProductQuery : ICommand<IEnumerable<ProductDtoV5>>
    {
        public String ProductCode { get; set; }
        public Guid InnCodeId { get; set; }
    }
    public class GetEquivalencesProductQueryHandler : ICommandHandler<GetEquivalencesProductQuery, IEnumerable<ProductDtoV5>> {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly MedIJKModel _model;

        public GetEquivalencesProductQueryHandler(IRepository<Product, Guid> productRepository, 
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
        public async Task<IEnumerable<ProductDtoV5>> Handle(GetEquivalencesProductQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery {OrganizationId = orgId.Value}, cancellationToken);
            var supplier = await _commandBus.SendAsync(new GetByIdOfSupplierQuery {Id = customer.SupplierId}, cancellationToken);
            orgId = supplier.OrganizationId;
            if (!orgId.HasValue)
                throw new InvalidOperationException("");
            var result = _productRepository.Table
                .Include(x => x.Manufacturer)
                .Include(x => x.InnCode)
                .Include(x => x.ProductClass)
                .Include(x => x.TaxGroup)
                .Include(x => x.List)
                .Where(x => x.ProductState == ProductState.Valid)
                .Where(x => x.Code != request.ProductCode && x.INNCodeId == request.InnCodeId);
           
          
            var data  = await result
               
                .Select(p => new ProductDtoV5
                {
                    FullName = p.FullName,
                    Id = p.Id,
                    Code = p.Code,
                    Manufacturer = p.Manufacturer != null ? p.Manufacturer.Name : "",
                    TaxGroup = p.TaxGroup != null ? p.TaxGroup.Name : "",
                    Tax = p.TaxGroup != null ? p.TaxGroup.TaxValue / 100 : 0,
                    Psychotropic = p.Psychotropic,
                    ProductClassName = p.ProductClass != null ? p.ProductClass.Name : "",
                    ProductClassId = p.ProductClassId,
                    INNCodeId = p.INNCodeId,
                    INNCodeName = p.InnCode != null ? p.InnCode.Name : "",
                    HasQuota = p.Quota,
                    PickingZoneId = p.PickingZoneId,
                    Thermolabile = p.Thermolabile,
                    DefaultLocation = p.DefaultLocation,
                    PFS = p.List != null ? p.List.SHP : 0,
                    DciFullName = p.DciConcat,
                    Images = p.Images.Select(c => Convert.ToBase64String(c.ImageBytes)).ToList()

                })
                .ToListAsync(cancellationToken);
            
            var availableProducts = await _commandBus.SendAsync(new GetAvailableQuantitiesInStockQuery
                { ProductIds = data.Select(x => x.Id).ToList(), SupplierId = orgId }, cancellationToken);
            
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

            return data;
        }
    }
}