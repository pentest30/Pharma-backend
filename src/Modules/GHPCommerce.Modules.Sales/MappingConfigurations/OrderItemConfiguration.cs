using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");//.ValueGeneratedNever();
            builder.Property(m => m.UnitPriceInclTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TotalExlTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TotalInlTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaPFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");

        }
    }
}
