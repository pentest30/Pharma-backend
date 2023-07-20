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
    public class InventSumModelV2
    {
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double PhysicalOnHandQuantity { get; set; }
        public bool IsPublic { get; set; }
        public string PackagingCode { get; set; }
        public decimal PFS { get; set; }

        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
    }
    public class InventSumModelV2ConfigMapping : Profile
    {
        public InventSumModelV2ConfigMapping()
        {
            CreateMap<InventSumModelV2, CreateAXInventSumCommand>().ReverseMap();
            CreateMap<InventSumModelV2, InventoryDimensionExistsQueryV2>().ReverseMap();
        }
    }
    public class InventSumModelV2Validator : AbstractValidator<InventSumModelV2>
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public InventSumModelV2Validator(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
            RuleFor(v => v.PhysicalOnHandQuantity)
                .NotNull()
                .GreaterThan(-1);
           
            // RuleFor(v => v.ProductCode)
            //     .NotEmpty()
            //     .MustAsync(ActiveProduct)
            //     .WithMessage("Produit inexistant ou bien  invalide").WithErrorCode("InvalidProduct");
            // //RuleFor(v => v)
            //    .MustAsync(ValidateUniqueness)
            //    .WithMessage("Erreur Dimension :Produit en stock avec les mêmes dimensions,Vous l'alimentez?")
            //    .WithErrorCode("DuplicatedDimensions");


        }

        private async Task<bool> ActiveProduct(string arg1, CancellationToken arg2)
        {
            return await _commandBus.SendAsync(new ActiveProductExistsByCode { Code = arg1 }, arg2);
        }

        private async Task<bool> ValidateUniqueness(InventSumModelV2 model, CancellationToken token)
        {
            return await _commandBus.SendAsync(_mapper.Map<InventoryDimensionExistsQueryV2>(model), token);
        }

       
    }
}
