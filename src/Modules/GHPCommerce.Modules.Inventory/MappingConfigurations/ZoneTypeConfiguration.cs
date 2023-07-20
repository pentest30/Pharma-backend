using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class ZoneTypeConfiguration : IEntityTypeConfiguration<ZoneType>
    {
        public void Configure(EntityTypeBuilder<ZoneType> builder)
        {
            builder.ToTable("ZoneType", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasData(new List<ZoneType>
            {
                new ZoneType {Name = "Vendable", Id = Guid.Parse("6BD42E21-E657-4F99-AFEF-1AFE5CEACB16")},
                new ZoneType {Name = "Non vendable", Id = Guid.Parse("6AD42E21-E657-4F99-AFEF-1AFE5CEACB16")}

            });
        }
    }
}
