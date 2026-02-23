using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateExtensionMap : IEntityTypeConfiguration<PatDueDateExtension>
    {
        public void Configure(EntityTypeBuilder<PatDueDateExtension> builder)
        {
            builder.ToTable("tblPatDueDateExtension");
            builder.HasOne(c => c.PatDueDate).WithMany(d => d.PatDueDateExtensions).HasForeignKey(c => c.DDId).HasPrincipalKey(c => c.DDId);
        }
    }

    public class DueDateExtensionLogMap : IEntityTypeConfiguration<DueDateExtensionLog>
    {
        public void Configure(EntityTypeBuilder<DueDateExtensionLog> builder)
        {
            builder.ToTable("tblDueDateExtensionLog");
        }
    }
}
