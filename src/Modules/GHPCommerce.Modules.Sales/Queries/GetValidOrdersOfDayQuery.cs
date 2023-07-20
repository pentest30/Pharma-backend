using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetValidOrdersOfDayQuery  : ICommand<List<OrderByDateDto>>
    {
        
    }
    public class GetValidOrdersOfDayQueryHandler : ICommandHandler<GetValidOrdersOfDayQuery, List<OrderByDateDto>>
    {
        private readonly IRepository<Order, Guid> _repository;

        public GetValidOrdersOfDayQueryHandler(IRepository<Order, Guid> repository)
        {
            _repository = repository;
        }
        public async Task<List<OrderByDateDto>> Handle(GetValidOrdersOfDayQuery request, CancellationToken cancellationToken)
        {
            var query = await _repository.Table
                .Where(x => x.CreatedDateTime.Date == DateTimeOffset.Now.Date &&
                            (x.OrderStatus != OrderStatus.Canceled && x.OrderStatus != OrderStatus.CanceledAx))
                .Select(x => new OrderByDateDto
                {
                    CommandType = x.OrderType == OrderType.Psychotrope ? "Psychotrope" : "Non psychotrope",
                    CreatedBy = x.CreatedBy,
                    CustomerName = x.CustomerName,
                    TotalIncludeDiscount = x.OrderDiscount,
                    OrderNumber = x.OrderNumberSequence.ToString(),
                    OrderTotal = x.OrderTotal,
                    OrderStatus = GetOrderStatus((uint)x.OrderStatus)
                    
                }).ToListAsync(cancellationToken: cancellationToken);
            return query;

        }
        private static string GetOrderStatus(uint status)
        {
            string str = status switch
            {
                10 => "EN ATTENTE",
                20 => "Envoyée",
                30 => "Acceptée/Confirmée",
                40 => "En cours de traitement",
                50 => "En route",
                60 => "Terminée",
                70 => "Annulée",
                80 => "Rejetée",
                90 => "Confirmé / Imprimée",
                100 => "Consolidée",
                110 => "En zone d'expédition",
                120 => "Confirmée",
                130 => "En cours de chargement",
                140 => "Facturée",
                150 => "En cours de prélèvement",
                160 => "Prélevée",
                170 => "Accusé de réception",
                180 => "Erreur de syncronisation",
                190 => "Expédiée",
                200 => "Annulée sur AX",
                210 => "Partiellement créée sur AX",
                _ => string.Empty
            };

            return str;
        }
    }
}