using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class PreparationOrderVerifierConfiguration : IEntityTypeConfiguration<Entities.PreparationOrderVerifier>
    {
        public void Configure(EntityTypeBuilder<Entities.PreparationOrderVerifier> builder)
        {
            builder.ToTable("PreparationOrderVerifier", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}
