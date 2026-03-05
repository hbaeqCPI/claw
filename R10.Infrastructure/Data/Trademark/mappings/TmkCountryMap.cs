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
            builder.HasMany(c => c.TmkCountryAreas).WithOne(ca => ca.AreaCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca => ca.Country);
        }
    }
}
