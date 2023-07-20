using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class ManifacturerConfiguration: IEntityTypeConfiguration<Manufacturer>
    {
        public void Configure(EntityTypeBuilder<Manufacturer> builder)
        {
            builder.ToTable("Manufacturers", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Addresses);
                //.WithOne(x => x.Manufacturer);
                builder.HasMany(x => x.PhoneNumbers);
                //.WithOne(x=>x.Manufacturer);
                builder.HasMany(x => x.Emails);
                //.WithOne(x => x.Manufacturer);
            builder.HasMany(x => x.Products);
        }
    }
}
