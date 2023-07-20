using System;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.ProductClasses.Queries
{
    public class GetProductClassByIdQuery :ICommand<ProductClassDto>
    {
        public Guid Id { get; set; }
    }
}
