using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class SectorCustomerConfiguration : IEntityTypeConfiguration<SectorCustomer>
    {
        public void Configure(EntityTypeBuilder<SectorCustomer> builder)
        {
            builder.ToTable("Sectors", schema: "Tiers");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
           
        }
    }
}
