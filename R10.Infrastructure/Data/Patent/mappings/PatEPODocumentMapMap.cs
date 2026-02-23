using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMapMap : IEntityTypeConfiguration<PatEPODocumentMap>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMap> builder)
        {
            builder.ToTable("tblPatEPODocumentMap");
            builder.HasKey(d => new { d.MapId });            
            builder.HasIndex(d => new { d.DocumentCode, d.DocumentName, d.Language }).IsUnique();
        }
    }
}
