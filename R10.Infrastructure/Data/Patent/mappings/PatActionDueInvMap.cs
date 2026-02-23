using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatActionDueInvMap : IEntityTypeConfiguration<PatActionDueInv>
    {
        public void Configure(EntityTypeBuilder<PatActionDueInv> builder)
        {
            builder.ToTable("tblPatActionDueInv");
            builder.HasIndex(a => new { a.InvId, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasIndex(a => new { a.CaseNumber, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasOne(a => a.Invention).WithMany(c => c.ActionDueInvs).HasForeignKey(a => a.InvId).HasPrincipalKey(c => c.InvId);
        }
    }
}
