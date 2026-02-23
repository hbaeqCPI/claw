using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSStatusChangeLogMap : IEntityTypeConfiguration<AMSStatusChangeLog>
    {
        public void Configure(EntityTypeBuilder<AMSStatusChangeLog> builder)
        {
            builder.ToTable("tblAMSStatusChangeLog");
            builder.HasKey(l => l.LogID);
            builder.HasOne(l => l.AMSDue).WithMany(d => d.AMSStatusChangeLog).HasForeignKey(l => l.DueID).HasPrincipalKey(d => d.DueID);
            builder.HasOne(l => l.AMSMain).WithMany(m => m.AMSStatusChangeLog).HasForeignKey(l => l.AnnID).HasPrincipalKey(m => m.AnnID);
        }
    }
}
