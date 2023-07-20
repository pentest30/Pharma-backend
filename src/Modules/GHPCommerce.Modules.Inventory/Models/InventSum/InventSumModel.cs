using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Inventory.Commands;
using GHPCommerce.Modules.Inventory.Queries;

namespace GHPCommerce.Modules.Inventory.Models.InventSum
{
    
    public class InventSumModel
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductFullName { get; set; }
        public string ProductCode { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? BestBeforeDate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double PhysicalOnhandQuantity { get; set; }
        public double PhysicalReservedQuantity { get; set; }
        public bool? IsPublic { get; set; }
        public Guid? SiteId { get; set; }
        public string SiteName { get; set; }
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double? MinThresholdAlert { get; set; }
        public string PackagingCode { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
    }

    public class InventSumModelConfigMapping : Profile
    {
        public InventSumModelConfigMapping()
        {
            CreateMap<InventSumModel, CreateInventSumCommand>().ReverseMap();
            CreateMap<InventSumModel, InventoryDimensionExistsQuery>().ReverseMap();
            CreateMap<InventSumModel, UpdateInventSumCommand>().ReverseMap();
            
        }
    }

    public class InventSumModelValidator : AbstractValidator<InventSumModel>
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventSumModelValidator(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
            RuleFor(v => v.PhysicalOnhandQuantity)
                .NotNull()
                .NotEqual(0);
            RuleFor(v => v.ProductId)
                .NotEqual(Guid.Empty)
                .When(x => string.IsNullOrEmpty(x.ProductCode))
                .MustAsync(ActiveProductExists)
                .WithMessage("Produit inexistant ou bien  invalide")
                .WithErrorCode("InvalidProduct");
         
            RuleFor(v => v)
                .MustAsync(ValidateUniqueness)
                .WithMessage("Erreur Dimension :Produit en stock avec les mêmes dimensions,Vous l'alimentez?")
                .WithErrorCode("DuplicatedDimensions");
            

        }

        private async Task<bool> ActiveProduct(string arg1, CancellationToken arg2)
        {
            return await _commandBus.SendAsync(new ActiveProductExistsByCode {Code  = arg1}, arg2);
        }

        private async Task<bool> ValidateUniqueness(InventSumModel model, CancellationToken token)
        {
            return await _commandBus.SendAsync(_mapper.Map<InventoryDimensionExistsQuery>(model), token);
        }

        private async Task<bool> ActiveProductExists(InventSumModel model, Guid value, CancellationToken token)
        {
            return await _commandBus.SendAsync(new ActiveProductExists {Id = model.ProductId}, token);
        }
    }
}
