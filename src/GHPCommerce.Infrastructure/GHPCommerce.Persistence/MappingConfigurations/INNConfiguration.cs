using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class INNConfiguration: IEntityTypeConfiguration<INN>
    {
        public void Configure(EntityTypeBuilder<INN> builder)
        {
            builder.ToTable("INNs", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            
        }
    }
}
