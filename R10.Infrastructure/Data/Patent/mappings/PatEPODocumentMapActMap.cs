using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMapActMap : IEntityTypeConfiguration<PatEPODocumentMapAct>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMapAct> builder)
        {
            builder.ToTable("tblPatEPODocumentMapAct");
            builder.HasKey(d => new { d.MapDueId });            
            builder.HasIndex(d => new { d.DocumentCode, d.ActionType, d.ActionDue, d.Yr, d.Mo, d.Dy }).IsUnique();
        }
    }
}
