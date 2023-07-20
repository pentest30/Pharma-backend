using System;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Queries
{
    public class GetTherapeuticClassByIdQuery :ICommand<TherapeuticClassDto>
    {
        public Guid Id { get; set; }

    }
}
