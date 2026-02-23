using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDueDateExtensionMap : IEntityTypeConfiguration<DMSDueDateExtension>
    {
        public void Configure(EntityTypeBuilder<DMSDueDateExtension> builder)
        {
            builder.ToTable("tblDMSDueDateExtension");
            builder.HasOne(c => c.DMSDueDate).WithMany(d => d.DMSDueDateExtensions).HasForeignKey(c => c.DDId).HasPrincipalKey(c => c.DDId);
        }
    }
}
