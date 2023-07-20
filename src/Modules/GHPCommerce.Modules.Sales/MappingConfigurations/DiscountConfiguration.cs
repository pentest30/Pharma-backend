using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.ToTable("Discount", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(m => m.DiscountRate).HasColumnType("decimal(18,4)");
            builder.HasIndex(x => new
            {
                x.OrganizationId,
                x.ProductId,
                x.DiscountRate,
                x.ThresholdQuantity,
                x.from,
                x.to
            }).HasName("IX_SalesDiscount").IsClustered(false).IsUnique().HasFilter(null);
        }
    }
}
