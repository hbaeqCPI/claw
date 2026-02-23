using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostTrackingImportMappingMap : IEntityTypeConfiguration<TmkCostTrackingImportMapping>
    {
        public void Configure(EntityTypeBuilder<TmkCostTrackingImportMapping> builder)
        {
            builder.ToTable("tblTmkCostImportMapping");
            
        }
    }
}
