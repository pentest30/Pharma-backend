using System;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetSupplierByIdQuery : ICommand<SupplierDto>
    {
        public Guid SupplierId { get; set; }
    }
}