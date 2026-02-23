using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWFieldUsageMap : IEntityTypeConfiguration<FormIFWFieldUsage>
    {
        public void Configure(EntityTypeBuilder<FormIFWFieldUsage> builder)
        {
            builder.ToTable("tblFRIFWFieldUsage");
            builder.HasMany(u=>u.FormIFWDataExtracts).WithOne(e=>e.FormIFWFieldUsage).HasForeignKey(e=>e.UsageId).HasPrincipalKey(u=>u.UsageId);

        }
    }
}
