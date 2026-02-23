using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSAgendaRelatedDisclosureMap : IEntityTypeConfiguration<DMSAgendaRelatedDisclosure>
    {
        public void Configure(EntityTypeBuilder<DMSAgendaRelatedDisclosure> builder)
        {

            builder.ToTable("tblDMSAgendaRelatedDisclosure");
            builder.HasIndex(r => new { r.AgendaId, r.DMSId }).IsUnique();
            builder.HasOne(r => r.DMSAgenda).WithMany(dms => dms.DMSAgendaRelatedDisclosures).HasForeignKey(r => r.AgendaId).HasPrincipalKey(d => d.AgendaId);
            builder.HasOne(r => r.Disclosure).WithMany(dms => dms.DMSAgendaRelatedDisclosures).HasForeignKey(r => r.DMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
