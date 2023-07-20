﻿using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductSharedQuery : ICommand<IEnumerable<ProductDtoV3>>
    {
    }
}