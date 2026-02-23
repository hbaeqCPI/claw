using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDueDateDelegationMap : IEntityTypeConfiguration<DMSDueDateDelegation>
    {
        public void Configure(EntityTypeBuilder<DMSDueDateDelegation> builder)
        {
            builder.ToTable("tblDMSDueDateDelegation");
            builder.HasIndex(a => new { a.ActId, a.DDId, a.GroupId, a.UserId }).IsUnique();
            builder.HasOne(a => a.DMSActionDue).WithMany(a => a.Delegations).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.DMSDueDate).WithMany(a => a.Delegations).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
