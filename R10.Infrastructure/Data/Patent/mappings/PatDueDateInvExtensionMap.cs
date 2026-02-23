using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvExtensionMap : IEntityTypeConfiguration<PatDueDateInvExtension>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInvExtension> builder)
        {
            builder.ToTable("tblPatDueDateInvExtension");
            builder.HasOne(c => c.PatDueDateInv).WithMany(d => d.PatDueDateInvExtensions).HasForeignKey(c => c.DDId).HasPrincipalKey(c => c.DDId);
        }
    }

}
