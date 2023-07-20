using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organizations", schema: "Tiers");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.UserAccounts)
                .WithOne(u=>u.Organization)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Addresses)
                .WithOne(x=>x.Organization);
            builder.HasMany(x => x.PhoneNumbers)
                .WithOne(x=>x.Organization);
            builder.HasMany(x => x.Emails)
                .WithOne(x => x.Organization);
            builder.HasMany(x => x.BankAccounts)
                .WithOne(x=>x.Organization)
                .OnDelete(DeleteBehavior.Cascade);
           

        }
    }
}
