using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMergeMap : IEntityTypeConfiguration<PatEPODocumentMerge>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMerge> builder)
        {
            builder.ToTable("tblPatEPODocumentMerge");
            builder.HasKey(d => new { d.MergeId });
            builder.HasIndex(d => d.MergeName).IsUnique();            
        }
    }
}
