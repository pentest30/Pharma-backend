using System;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Dosages.Queries
{
    public class GetDosageByIdQuery  : ICommand<DosageDto>
    {
        public Guid Id { get; set; }
    }
}
