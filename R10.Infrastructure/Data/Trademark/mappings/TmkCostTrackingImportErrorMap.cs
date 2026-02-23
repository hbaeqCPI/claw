using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;


namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostTrackingImportErrorMap : IEntityTypeConfiguration<TmkCostTrackingImportError>
    {
        public void Configure(EntityTypeBuilder<TmkCostTrackingImportError> builder)
        {
            builder.ToTable("tblTmkCostImportErrorLog");
            
        }
    }
}
