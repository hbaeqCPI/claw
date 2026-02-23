using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkConflictStatusMap : IEntityTypeConfiguration<TmkConflictStatus>
    {
        public void Configure(EntityTypeBuilder<TmkConflictStatus> builder)
        {
            builder.ToTable("tblTmkConflictStatus");
            builder.Property(s => s.ConflictStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.ConflictStatusId).UseIdentityColumn();
            builder.Property(s => s.ConflictStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(s => s.TmkConflicts).WithOne(c => c.TmkConflictStatus).HasForeignKey(s => s.ConflictStatus).HasPrincipalKey(c=>c.ConflictStatus);
            
        }
    }
}
