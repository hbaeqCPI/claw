using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMDueDateExtensionMap : IEntityTypeConfiguration<GMDueDateExtension>
    {
        public void Configure(EntityTypeBuilder<GMDueDateExtension> builder)
        {
            builder.ToTable("tblGMDueDateExtension");
            builder.HasOne(c => c.GMDueDate).WithMany(d => d.GMDueDateExtensions).HasForeignKey(c => c.DDId).HasPrincipalKey(c => c.DDId);
        }
    }
}
