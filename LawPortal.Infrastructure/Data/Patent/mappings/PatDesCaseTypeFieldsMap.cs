using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeFieldsMap : IEntityTypeConfiguration<PatDesCaseTypeFields>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeFields> builder)
        {
            builder.ToTable("tblPatDesCaseTypeFields");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
