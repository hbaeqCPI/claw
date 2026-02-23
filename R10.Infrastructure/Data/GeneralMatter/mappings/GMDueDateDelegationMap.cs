using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMDueDateDelegationMap : IEntityTypeConfiguration<GMDueDateDelegation>
    {
        public void Configure(EntityTypeBuilder<GMDueDateDelegation> builder)
        {
            builder.ToTable("tblGMDueDateDelegation");
            builder.HasIndex(a => new { a.ActId, a.DDId, a.GroupId, a.UserId }).IsUnique();
            builder.HasOne(a => a.GMActionDue).WithMany(a => a.Delegations).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.GMDueDate).WithMany(a => a.Delegations).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
