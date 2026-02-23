using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;


namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTrackingImportErrorMap : IEntityTypeConfiguration<GMCostTrackingImportError>
    {
        public void Configure(EntityTypeBuilder<GMCostTrackingImportError> builder)
        {
            builder.ToTable("tblGMCostImportErrorLog");
            
        }
    }
}
