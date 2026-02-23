using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaCountryMap : IEntityTypeConfiguration<TmkAreaCountry>
    {
        public void Configure(EntityTypeBuilder<TmkAreaCountry> builder)
        {
            builder.ToTable("tblTmkAreaCountry");
            builder.HasIndex(a => new { a.AreaID, a.Country }).IsUnique();
        }
    }
}
