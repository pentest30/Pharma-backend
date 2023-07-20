using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    class AllowedProductClassConfiguration : IEntityTypeConfiguration<AllowedProductClass>
    {
        public void Configure(EntityTypeBuilder<AllowedProductClass> builder)
        {
            builder.ToTable("AllowedProductClasses", schema: "Tiers");
            builder.HasKey(x => new { x.SupplierCustomerId, x.ProductClassId  });
        }
    }
}
