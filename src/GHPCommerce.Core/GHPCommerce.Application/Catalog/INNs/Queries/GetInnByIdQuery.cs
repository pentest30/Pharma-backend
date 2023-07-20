using System;
using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNs.Queries
{
    public class GetInnByIdQuery : ICommand<InnDto>
    {
        public Guid Id { get; set; }
    }
}
