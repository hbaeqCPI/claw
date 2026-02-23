using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacClearanceStatusMap : IEntityTypeConfiguration<PacClearanceStatus>
    {
        public void Configure(EntityTypeBuilder<PacClearanceStatus> builder)
        {
            builder.ToTable("tblPacClearanceStatus");
            builder.Property(s => s.ClearanceStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.ClearanceStatusId).UseIdentityColumn();
            builder.Property(s => s.ClearanceStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.ClearanceStatus).IsUnique();            
            builder.HasMany(a => a.Clearances).WithOne(c => c.PacClearanceStatus).HasForeignKey(ca => ca.ClearanceStatus).HasPrincipalKey(a => a.ClearanceStatus);
        }
    }
}
