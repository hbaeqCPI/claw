using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesignatedCountryMap : IEntityTypeConfiguration<TmkDesignatedCountry>
    {
        public void Configure(EntityTypeBuilder<TmkDesignatedCountry> builder)
        {
            builder.ToTable("tblTmkDesignatedCountry");
            builder.HasOne(h => h.Country).WithMany(c => c.TmkDesignatedCountries).HasPrincipalKey(c => c.Country)
                .HasForeignKey(h => h.DesCountry);
        }
    }
}
