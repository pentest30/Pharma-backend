using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    class StockZoneConfiguration: IEntityTypeConfiguration<StockZone>
    {
        public void Configure(EntityTypeBuilder<StockZone> builder)
        {
            builder.ToTable("StockZone", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasData(new List<StockZone>
            {
                new StockZone {Id= Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16"),Name = "Zone vendable", ZoneTypeId = Guid.Parse("6BD42E21-E657-4F99-AFEF-1AFE5CEACB16"), ZoneState = EntityStatus.Active},
                new StockZone {Id= Guid.Parse("7BD42E22-E657-4F99-AFEF-1AFE5CEACB16"),Name = "Zone non vendable", ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16") , ZoneState = EntityStatus.Active},
                new StockZone {Id= Guid.Parse("7BD42E23-E657-4F99-AFEF-1AFE5CEACB16"),Name = "Zone Chez le fournisseur", ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16") , ZoneState = EntityStatus.Active}
            });
        }
    }
}
