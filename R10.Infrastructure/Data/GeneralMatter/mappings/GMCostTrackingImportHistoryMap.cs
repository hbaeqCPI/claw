using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTrackingImportHistoryMap : IEntityTypeConfiguration<GMCostTrackingImportHistory>
    {
        public void Configure(EntityTypeBuilder<GMCostTrackingImportHistory> builder)
        {
            builder.ToTable("tblGMCostImportHistory");
            //builder.HasOne(h => h.DataType).WithMany(t => t.Imports);
        }
    }
}
