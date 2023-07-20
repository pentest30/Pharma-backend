using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class PreparationOrderExecuterConfiguration : IEntityTypeConfiguration<Entities.PreparationOrderExecuter>
    {
        public void Configure(EntityTypeBuilder<Entities.PreparationOrderExecuter> builder)
        {
            builder.ToTable("PreparationOrderExecuter", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}
