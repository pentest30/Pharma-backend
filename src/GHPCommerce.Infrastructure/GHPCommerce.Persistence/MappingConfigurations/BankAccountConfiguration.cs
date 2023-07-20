using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("BankAccount", schema: "Tiers");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}
