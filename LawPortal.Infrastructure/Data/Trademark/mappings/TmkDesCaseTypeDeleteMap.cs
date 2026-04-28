using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeDeleteMap : IEntityTypeConfiguration<TmkDesCaseTypeDelete>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeDelete> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeDelete");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.IntlCodeNew, e.CaseTypeNew, e.DesCountryNew, e.DesCaseTypeNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}