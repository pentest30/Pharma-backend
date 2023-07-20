using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.PreparationOrder.Commands
{
    public class CreatePreparationOrderCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
        public string OrderIdentifier { get; set; }
        public DateTime? OrderDate { get; set; }

        public Guid ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int ZoneGroupOrder { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SectorName { get; set; }
        public string SectorCode { get; set; }
        public string IdentifierNumber { get; set; }
        public string BarCode { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public byte[] BarCodeImage { get; set; }
        public int TotalZoneCount { get; set; }
        public int OrderNumberSequence { get; set; }
        public int zoneGroupOrder { get; set; }
        public bool ToBeRespected { get; set; }
        public string CodeAx { get; set; }

        public List<PreparationOrderItemDtoV1> PreparationOrderItems { get; set; }

    }

}
