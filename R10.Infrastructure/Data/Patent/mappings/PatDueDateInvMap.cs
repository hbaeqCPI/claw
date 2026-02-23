using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvMap : IEntityTypeConfiguration<PatDueDateInv>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInv> builder)
        {
            builder.ToTable("tblPatDueDateInv");
            builder.HasIndex(a => new { a.ActId, a.ActionDue, a.DueDate }).IsUnique();
            builder.HasOne(a => a.PatActionDueInv).WithMany(a => a.DueDateInvs).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.DeDocketOutstanding).WithOne(a => a.PatDueDateInv).HasForeignKey<PatDueDateInvDeDocketOutstanding>(a => a.DDId).HasPrincipalKey<PatDueDateInv>(a => a.DDId);
        }
    }
}
