using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.AMS;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSRemLogErrorMap : IEntityTypeConfiguration<RemLogError<AMSDue, AMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogError<AMSDue, AMSRemLogDue>> builder)
        {
            builder.ToTable("tblAMSRemLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogErrors).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
