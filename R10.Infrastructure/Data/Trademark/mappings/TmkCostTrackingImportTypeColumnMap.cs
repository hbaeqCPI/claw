using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostTrackingImportTypeColumnMap : IEntityTypeConfiguration<TmkCostTrackingImportTypeColumn>
    {
        public void Configure(EntityTypeBuilder<TmkCostTrackingImportTypeColumn> builder)
        {
            builder.HasNoKey().ToView("vwTmkCostTrackingImportTypeColumns");
            
        }
    }

   
}
