using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryMap : IEntityTypeConfiguration<TmkCountry>
    {
        public void Configure(EntityTypeBuilder<TmkCountry> builder)
        {
            builder.ToTable("tblTmkCountry");
            builder.HasKey(e => e.Country);
            builder.Ignore(e => e.TmkCountryAreas);
        }
    }
}
