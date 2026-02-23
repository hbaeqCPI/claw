using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackingImportHistoryMap : IEntityTypeConfiguration<PatCostTrackingImportHistory>
    {
        public void Configure(EntityTypeBuilder<PatCostTrackingImportHistory> builder)
        {
            builder.ToTable("tblPatCostImportHistory");
            //builder.HasOne(h => h.DataType).WithMany(t => t.Imports);
        }
    }
}
