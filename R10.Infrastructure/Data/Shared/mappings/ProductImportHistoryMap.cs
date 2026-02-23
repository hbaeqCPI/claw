using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ProductImportHistoryMap : IEntityTypeConfiguration<ProductImportHistory>
    {
        public void Configure(EntityTypeBuilder<ProductImportHistory> builder)
        {
            builder.ToTable("tblPrdProductImportHistory");
            //builder.HasOne(h => h.DataType).WithMany(t => t.Imports);
        }
    }
}
