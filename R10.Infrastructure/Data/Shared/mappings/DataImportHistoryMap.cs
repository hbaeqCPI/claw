using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataImportHistoryMap : IEntityTypeConfiguration<DataImportHistory>
    {
        public void Configure(EntityTypeBuilder<DataImportHistory> builder)
        {
            builder.ToTable("tblDataImportHistory");
            builder.HasOne(h => h.DataType).WithMany(t => t.Imports);
        }
    }
}
