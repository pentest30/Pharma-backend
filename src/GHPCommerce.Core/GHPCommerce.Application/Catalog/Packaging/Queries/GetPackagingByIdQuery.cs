using System;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Packaging.Queries
{
    public class GetPackagingByIdQuery :ICommand<PackagingDto>
    {
        public Guid Id { get; set; }
    }
}
