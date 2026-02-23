using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;


namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ProductImportErrorMap : IEntityTypeConfiguration<ProductImportError>
    {
        public void Configure(EntityTypeBuilder<ProductImportError> builder)
        {
            builder.ToTable("tblPrdProductImportErrorLog");
            
        }
    }
}
