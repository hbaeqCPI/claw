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
            // builder.HasOne(h => h.Country).WithMany().HasPrincipalKey(c => c.Country)
            //     .HasForeignKey(h => h.DesCountry); // Removed: Country nav property no longer exists
        }
    }
}
