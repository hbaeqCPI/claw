using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackingImportTypeColumnMap : IEntityTypeConfiguration<PatCostTrackingImportTypeColumn>
    {
        public void Configure(EntityTypeBuilder<PatCostTrackingImportTypeColumn> builder)
        {
            builder.HasNoKey().HasNoKey().ToView("vwPatCostTrackingImportTypeColumns");
            
        }
    }

   
}
