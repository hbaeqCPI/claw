using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAssignmentHistoryMap : IEntityTypeConfiguration<TmkAssignmentHistory>
    {
        public void Configure(EntityTypeBuilder<TmkAssignmentHistory> builder)
        {
            builder.ToTable("tblTmkAssignmentHistory");
            builder.HasIndex(a => new { a.TmkId, a.AssignmentFrom, a.AssignmentTo, a.AssignmentDate, a.Reel, a.Frame });
            builder.HasIndex(a => a.AssignmentFrom);
            builder.HasIndex(a => a.AssignmentTo);
            builder.HasOne(a => a.TmkTrademark).WithMany(t => t.AssignmentsHistory).HasForeignKey(h => h.TmkId).HasPrincipalKey(c => c.TmkId);
        }
    }
}
