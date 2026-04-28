using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaCountryDeleteMap : IEntityTypeConfiguration<TmkAreaCountryDelete>
    {
        public void Configure(EntityTypeBuilder<TmkAreaCountryDelete> builder)
        {
            builder.ToTable("tblTmkAreaCountryDelete");
            builder.HasKey(e => new { e.Area, e.Country, e.AreaNew, e.CountryNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}