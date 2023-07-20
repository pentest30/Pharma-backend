using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class ProductHasQuotaCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid CatalogId { get; set; }
    }
}
