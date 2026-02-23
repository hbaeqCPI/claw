using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostTrackingImportHistoryMap : IEntityTypeConfiguration<TmkCostTrackingImportHistory>
    {
        public void Configure(EntityTypeBuilder<TmkCostTrackingImportHistory> builder)
        {
            builder.ToTable("tblTmkCostImportHistory");
            //builder.HasOne(h => h.DataType).WithMany(t => t.Imports);
        }
    }
}
