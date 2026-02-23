using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSStatusTypeMap : IEntityTypeConfiguration<AMSStatusType>
    {
        public void Configure(EntityTypeBuilder<AMSStatusType> builder)
        {
            builder.ToTable("tblAMSStatusType");
            builder.HasKey(s => s.CPIStatus);
            builder.HasIndex(s => s.Description).IsUnique(false);
            builder.HasIndex(s => s.ClientApplicationStatus).IsUnique(false);
            builder.HasOne(s => s.PatApplicationStatus).WithMany(app => app.AMSStatusTypes).HasForeignKey(app => app.ClientApplicationStatus).HasPrincipalKey(s => s.ApplicationStatus);
        }
    }
}
