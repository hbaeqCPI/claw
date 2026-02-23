using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryMap : IEntityTypeConfiguration<TmkCountry>
    {
        public void Configure(EntityTypeBuilder<TmkCountry> builder)
        {
            builder.ToTable("tblTmkCountry");
            builder.Property(c => c.CountryID).ValueGeneratedOnAdd();
            builder.Property(c => c.CountryID).UseIdentityColumn();
            builder.Property(c => c.CountryID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(c => c.TmkTrademarks).WithOne(t => t.TmkCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca => ca.Country);
            builder.HasMany(c => c.TmkActionsDue).WithOne(app => app.TmkCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca => ca.Country);
            builder.HasMany(c => c.TmkPrioTrademarks).WithOne(t => t.TmkPrioCountry).HasPrincipalKey(c => c.Country).HasForeignKey(t => t.PriCountry);
            builder.HasMany(c => c.TmkCountryAreas).WithOne(ca => ca.AreaCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca => ca.Country);
            builder.HasMany(c => c.ClientDesignatedCountries).WithOne(cd => cd.TmkCountry).HasPrincipalKey(c => c.Country).HasForeignKey(cd => cd.DesCtry).IsRequired(false);
        }
    }
}
