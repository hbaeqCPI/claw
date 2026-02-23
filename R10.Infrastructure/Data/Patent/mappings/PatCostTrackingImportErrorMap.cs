using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;


namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackingImportErrorMap : IEntityTypeConfiguration<PatCostTrackingImportError>
    {
        public void Configure(EntityTypeBuilder<PatCostTrackingImportError> builder)
        {
            builder.ToTable("tblPatCostImportErrorLog");
            
        }
    }
}
