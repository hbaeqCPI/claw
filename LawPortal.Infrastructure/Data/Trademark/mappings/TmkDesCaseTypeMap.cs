using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeMap : IEntityTypeConfiguration<TmkDesCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseType> builder)
        {
            builder.ToTable("tblTmkDesCaseType");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
            builder.Ignore(e => e.ParentTStamp);
        }
    }
}
