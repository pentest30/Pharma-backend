using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public partial class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(m => m.OrderTotal).HasColumnType("decimal(18,2)");
            builder.Property(m => m.OrderDiscount).HasColumnType("decimal(18,2)");
            builder.Property
                    (o => o.OrderNumberSequence)
                .HasDefaultValueSql("NEXT VALUE FOR sales.OrderNumbers")
                .ValueGeneratedOnAdd();
        }
    }
}
