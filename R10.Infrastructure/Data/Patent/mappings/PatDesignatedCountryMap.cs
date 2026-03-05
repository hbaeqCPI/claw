using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesignatedCountryMap : IEntityTypeConfiguration<PatDesignatedCountry>
    {
        public void Configure(EntityTypeBuilder<PatDesignatedCountry> builder)
        {
            builder.ToTable("tblPatDesignatedCountry");
            builder.HasOne(h => h.Country).WithMany(c => c.PatDesignatedCountries).HasPrincipalKey(c => c.Country)
                .HasForeignKey(h => h.DesCountry);
        }
    }
}
