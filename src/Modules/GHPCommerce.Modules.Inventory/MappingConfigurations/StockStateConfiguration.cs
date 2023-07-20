using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class StockStateConfiguration : IEntityTypeConfiguration<StockState>
    {
        public void Configure(EntityTypeBuilder<StockState> builder)
        {
            builder.ToTable("StockState", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasData(new List<StockState>
            {
                new StockState
                {
                    Id = Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Libéré",
                    ZoneTypeId = Guid.Parse("6BD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Non libéré",
                    ZoneTypeId = Guid.Parse("6BD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD62E23-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Abîmé",
                    ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD72E23-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Périmé",
                    ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD82E23-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Sans vignette",
                    ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD92E23-E657-4F99-AFEF-1AFE5CEACB16"), Name = "Instance",
                    ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                },
                new StockState
                {
                    Id = Guid.Parse("7BD13E23-E657-4F99-AFEF-1AFE5CEACB16"), Name = "RAL",
                    ZoneTypeId = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16"), StockStatus = EntityStatus.Active
                }

            });
        }
    }
}
