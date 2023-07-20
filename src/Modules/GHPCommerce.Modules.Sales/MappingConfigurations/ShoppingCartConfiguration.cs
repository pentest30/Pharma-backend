using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.ToTable("ShoppingCartItems", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(m => m.Total).HasColumnType("decimal(18,2)");
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}
