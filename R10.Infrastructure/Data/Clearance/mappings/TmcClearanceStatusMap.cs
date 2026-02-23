using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcClearanceStatusMap : IEntityTypeConfiguration<TmcClearanceStatus>
    {
        public void Configure(EntityTypeBuilder<TmcClearanceStatus> builder)
        {
            builder.ToTable("tblTmcClearanceStatus");
            builder.Property(s => s.ClearanceStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.ClearanceStatusId).UseIdentityColumn();
            builder.Property(s => s.ClearanceStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.ClearanceStatus).IsUnique();
            builder.HasMany(c => c.Clearances).WithOne(cs => cs.TmcClearanceStatus).HasForeignKey(cs => cs.ClearanceStatus).HasPrincipalKey(c => c.ClearanceStatus);
        }
    }
}
