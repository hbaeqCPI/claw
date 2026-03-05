using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryMap : IEntityTypeConfiguration<PatCountry>
    {
        public void Configure(EntityTypeBuilder<PatCountry> builder)
        {
            builder.ToTable("tblPatCountry");
            builder.Property(c => c.CountryID).ValueGeneratedOnAdd();
            builder.Property(c => c.CountryID).UseIdentityColumn();
            builder.Property(c => c.CountryID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(c => c.PatCountryAreas).WithOne(ca => ca.AreaCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca=>ca.Country);
        }
    }
}
