using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetPreparationOrderByIdQuery : ICommand<PreparationOrdersDtoV2>
    {
        public Guid Id { get; set; }

    }
}
