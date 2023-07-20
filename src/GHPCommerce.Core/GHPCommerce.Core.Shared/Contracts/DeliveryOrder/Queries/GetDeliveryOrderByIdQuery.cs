using System;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Queries
{
    public class GetDeliveryOrderByIdQuery : ICommand<DeliveryOrderDtoV2>
    {
        public Guid Id { get; set; }
    }
}