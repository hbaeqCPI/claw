using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryLawMap : IEntityTypeConfiguration<TmkCountryLaw>
    {
        public void Configure(EntityTypeBuilder<TmkCountryLaw> builder)
        {
            builder.ToTable("tblTmkCountryLaw");
            builder.HasKey(e => new { e.Country, e.CaseType, e.Systems });
            builder.Ignore(e => e.CopyOptions);
            builder.Ignore(e => e.TmkCountryDues);
        }
    }
}
