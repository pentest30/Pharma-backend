using System;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetByIdOfSupplierQuery : ICommand<SupplierDto>
    {
        public Guid Id { get; set; }

    }
}