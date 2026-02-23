using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateDelegationMap : IEntityTypeConfiguration<TmkDueDateDelegation>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateDelegation> builder)
        {
            builder.ToTable("tblTmkDueDateDelegation");
            builder.HasIndex(a => new { a.ActId, a.DDId, a.GroupId, a.UserId }).IsUnique();
            builder.HasOne(a => a.TmkActionDue).WithMany(a => a.Delegations).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.TmkDueDate).WithMany(a => a.Delegations).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
