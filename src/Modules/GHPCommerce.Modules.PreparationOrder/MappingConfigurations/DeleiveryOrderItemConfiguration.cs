using GHPCommerce.Modules.PreparationOrder.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class DeleiveryOrderItemConfiguration: IEntityTypeConfiguration<DeleiveryOrderItem>
    {
        public void Configure(EntityTypeBuilder<DeleiveryOrderItem> builder)
        {
            builder.ToTable("DeleiveryOrderItem", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");//.ValueGeneratedNever();
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PurchaseUnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            
        }
    }
}