using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCaseTypeMap : IEntityTypeConfiguration<PatCaseType>
    {
        public void Configure(EntityTypeBuilder<PatCaseType> builder)
        {
            builder.ToTable("tblPatCaseType");
            builder.HasKey(e => new { e.CaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
