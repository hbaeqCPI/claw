using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaCountryMap : IEntityTypeConfiguration<TmkAreaCountry>
    {
        public void Configure(EntityTypeBuilder<TmkAreaCountry> builder)
        {
            builder.ToTable("tblTmkAreaCountry");
            builder.HasKey(e => new { e.Area, e.Country, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
