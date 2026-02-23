using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateExtensionMap : IEntityTypeConfiguration<TmkDueDateExtension>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateExtension> builder)
        {
            builder.ToTable("tblTmkDueDateExtension");
            builder.HasOne(c => c.TmkDueDate).WithMany(d => d.TmkDueDateExtensions).HasForeignKey(c => c.DDId).HasPrincipalKey(c => c.DDId);
        }
    }
}
