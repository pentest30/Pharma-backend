using System;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Lists.Queries
{
    public class GetListByIdQuery : ICommand<ListDto>
    {
        public Guid Id { get; set; }
    }
}
