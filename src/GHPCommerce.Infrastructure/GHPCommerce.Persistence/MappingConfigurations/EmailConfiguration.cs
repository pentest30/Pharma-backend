using GHPCommerce.Domain.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class EmailConfiguration : IEntityTypeConfiguration<EmailModel>
    {
        public void Configure(EntityTypeBuilder<EmailModel> builder)
        {
            builder.ToTable("Emails", schema: "Shared");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(a => a.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}
