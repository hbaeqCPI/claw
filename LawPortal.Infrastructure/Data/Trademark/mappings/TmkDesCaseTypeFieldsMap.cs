using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsMap : IEntityTypeConfiguration<TmkDesCaseTypeFields>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFields> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFields");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
