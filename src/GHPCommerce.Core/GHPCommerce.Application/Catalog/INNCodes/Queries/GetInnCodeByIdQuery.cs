using System;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNCodes.Queries
{
    public class GetInnCodeByIdQuery : ICommand<InnCodeDto>
    {
        public Guid Id { get; set; }

    }
}
