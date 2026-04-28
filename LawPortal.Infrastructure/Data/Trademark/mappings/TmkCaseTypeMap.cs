using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkCaseTypeMap : IEntityTypeConfiguration<TmkCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkCaseType> builder)
        {
            builder.ToTable("tblTmkCaseType");
            builder.HasKey(e => new { e.CaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
