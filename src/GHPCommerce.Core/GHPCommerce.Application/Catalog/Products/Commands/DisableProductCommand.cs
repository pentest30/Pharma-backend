﻿using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class DisableProductCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid CatalogId { get; set; }
    }
}
