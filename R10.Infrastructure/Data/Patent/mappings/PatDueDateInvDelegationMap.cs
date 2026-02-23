using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvDelegationMap : IEntityTypeConfiguration<PatDueDateInvDelegation>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInvDelegation> builder)
        {
            builder.ToTable("tblPatDueDateInvDelegation");
            builder.HasIndex(a => new { a.ActId, a.DDId, a.GroupId, a.UserId }).IsUnique();
            builder.HasOne(a => a.PatActionDueInv).WithMany(a => a.Delegations).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.PatDueDateInv).WithMany(a => a.Delegations).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
