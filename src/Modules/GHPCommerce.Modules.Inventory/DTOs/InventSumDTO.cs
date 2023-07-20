using System;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Queries;

namespace GHPCommerce.Modules.Inventory.DTOs
{
    public class InventSumDto 
    {
        public Guid Id { get; set; }
   
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryDateShort { get; set; }

        public DateTime? BestBeforeDate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double? PhysicalOnhandQuantity { get; set; }
        public double PhysicalAvailableQuantity { get; set; }
        public double? PhysicalReservedQuantity { get; set; }
        public double? PhysicalDispenseQuantity { get; set; }
        public bool IsPublic { get; set; }
        public Guid? SiteId { get; set; }
        public string SiteName { get; set; }
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double? MinThresholdAlert { get; set; }
        public string PackagingCode { get; set; }
        public int Packing { get; set; }

        public decimal PFS { get; set; }

        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public string SupplierName { get; set; }
        public double Value { get; set; }
        public string Manufacturer { get; set; }
        public bool Quota { get; set; }
    }

    public class InventSumModelConfigMapping : Profile
    {
        public InventSumModelConfigMapping()
        {
            CreateMap<InventSumDto, InventSum>().ReverseMap();
            CreateMap<InventSum, InventSumDto>()
                .ForMember(x => x.ExpiryDateShort,m => m.MapFrom(o => o.ExpiryDate.HasValue ? o.ExpiryDate.Value.Date.ToShortDateString() : ""))
                .ForMember(x=> x.Value, m => m.MapFrom(o => o.PurchaseUnitPrice * o.PhysicalAvailableQuantity))
                .ReverseMap();

            CreateMap<InventoryDimensionExistsQuery, InventSumReservationDto>().ReverseMap();
            CreateMap<CachedInventSum, InventSumDto>().ReverseMap();
            CreateMap<CachedInventSum, InventSum>().ReverseMap();
            CreateMap<InventSum, InventSumReservationDto>().ReverseMap()
                .ForMember(x => x.PhysicalAvailableQuantity, opt => opt.Ignore());

        }
    }

}
