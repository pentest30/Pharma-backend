using System;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries
{
    public class GetPharmacologicalClassByIdQuery :ICommand<PharmacologicalClassDto>
    {
        public Guid Id { get; set; }
    }
}
