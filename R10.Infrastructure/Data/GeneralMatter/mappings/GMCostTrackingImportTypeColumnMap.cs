using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTrackingImportTypeColumnMap : IEntityTypeConfiguration<GMCostTrackingImportTypeColumn>
    {
        public void Configure(EntityTypeBuilder<GMCostTrackingImportTypeColumn> builder)
        {
            builder.HasNoKey().ToView("vwGMCostTrackingImportTypeColumns");
            
        }
    }

   
}
