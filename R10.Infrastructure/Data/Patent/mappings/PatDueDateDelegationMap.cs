using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateDelegationMap : IEntityTypeConfiguration<PatDueDateDelegation>
    {
        public void Configure(EntityTypeBuilder<PatDueDateDelegation> builder)
        {
            builder.ToTable("tblPatDueDateDelegation");
            builder.HasIndex(a => new { a.ActId, a.DDId, a.GroupId, a.UserId }).IsUnique();
            builder.HasOne(a => a.PatActionDue).WithMany(a => a.Delegations).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.PatDueDate).WithMany(a => a.Delegations).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
