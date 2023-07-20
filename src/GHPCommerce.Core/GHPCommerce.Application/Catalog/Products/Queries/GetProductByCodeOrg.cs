using System;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetProductByCodeOrg: ICommand<ProductDtoFromCode>
    {
        public Guid OrganizationId { get; set; }
        public string Code { get; set; }
    }
}
