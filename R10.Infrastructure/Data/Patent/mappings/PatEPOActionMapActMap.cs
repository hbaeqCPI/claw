using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPOActionMapActMap : IEntityTypeConfiguration<PatEPOActionMapAct>
    {
        public void Configure(EntityTypeBuilder<PatEPOActionMapAct> builder)
        {
            builder.ToTable("tblPatEPOActionMapAct");
            builder.HasKey(d => new { d.MapDueId });            
            builder.HasIndex(d => new { d.TermId, d.ActionType, d.ActionDue }).IsUnique();
            builder.HasOne(d => d.EPODueDateTerm).WithMany(a => a.PatEPOActionMapActs).HasForeignKey(d => d.TermId).HasPrincipalKey(d => d.TermId);
        }
    }
}
