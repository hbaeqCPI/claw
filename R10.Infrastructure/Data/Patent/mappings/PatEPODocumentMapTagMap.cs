using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMapTagMap : IEntityTypeConfiguration<PatEPODocumentMapTag>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMapTag> builder)
        {
            builder.ToTable("tblPatEPODocumentMapTag");
            builder.HasKey(d => new { d.MapTagId });            
            builder.HasIndex(d => new { d.DocumentCode, d.Tag }).IsUnique();
        }
    }
}
