using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
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
