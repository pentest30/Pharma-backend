using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Batches.Commands;
using GHPCommerce.Core.Shared.Contracts.Transactions;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class TransactionInventConfigMapping : Profile
    {
        public TransactionInventConfigMapping()
        {
            CreateMap<InventItemTransaction, CreateAtSupplierInventTransactionCommand>().ReverseMap();
            CreateMap<CreateBatchCommand, CreateAtSupplierInventTransactionCommand>().ReverseMap();
        }
    }
}