using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryMap : IEntityTypeConfiguration<TmkCountry>
    {
        public void Configure(EntityTypeBuilder<TmkCountry> builder)
        {
            builder.ToTable("tblTmkCountry");
            builder.HasKey(e => new { e.Country, e.Systems });
            builder.Ignore(e => e.TmkCountryAreas);
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
