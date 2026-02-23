using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ProductImportTypeColumnMap : IEntityTypeConfiguration<ProductImportTypeColumn>
    {
        public void Configure(EntityTypeBuilder<ProductImportTypeColumn> builder)
        {
            builder.HasNoKey().ToView("vwPrdProductImportTypeColumns");
            
        }
    }

   
}
