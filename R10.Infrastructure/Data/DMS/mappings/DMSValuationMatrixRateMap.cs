using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSValuationMatrixRateMap : IEntityTypeConfiguration<DMSValuationMatrixRate>
    {
        public void Configure(EntityTypeBuilder<DMSValuationMatrixRate> builder)
        {
            builder.ToTable("tblDMSValuationMatrixRate");
            builder.HasIndex(c => new { c.ValId, c.Rating }).IsUnique();
        }
    }
}
