using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTrackingImportMappingMap : IEntityTypeConfiguration<GMCostTrackingImportMapping>
    {
        public void Configure(EntityTypeBuilder<GMCostTrackingImportMapping> builder)
        {
            builder.ToTable("tblGMCostImportMapping");
            
        }
    }
}
