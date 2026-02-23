using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackingImportMappingMap : IEntityTypeConfiguration<PatCostTrackingImportMapping>
    {
        public void Configure(EntityTypeBuilder<PatCostTrackingImportMapping> builder)
        {
            builder.ToTable("tblPatCostImportMapping");
            
        }
    }
}
