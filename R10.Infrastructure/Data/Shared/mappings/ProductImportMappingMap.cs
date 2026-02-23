using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ProductImportMappingMap : IEntityTypeConfiguration<ProductImportMapping>
    {
        public void Configure(EntityTypeBuilder<ProductImportMapping> builder)
        {
            builder.ToTable("tblPrdProductImportMapping");
            
        }
    }
}
