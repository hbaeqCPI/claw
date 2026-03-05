using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryLawMap : IEntityTypeConfiguration<PatCountryLaw>
    {
        public void Configure(EntityTypeBuilder<PatCountryLaw> builder)
        {
            builder.ToTable("tblPatCountryLaw");
            builder.HasIndex(c => new { c.Country, c.CaseType }).IsUnique();
            builder.HasMany(c => c.PatCountryDues).WithOne(d => d.PatCountryLaw)
                .HasPrincipalKey(c => c.CountryLawID).HasForeignKey(d => d.CountryLawID);
            builder.HasOne(c => c.PatCaseType).WithMany(ct => ct.CaseTypeCountryLaws)
             .HasPrincipalKey(c => c.CaseType).HasForeignKey(d => d.CaseType);
            builder.HasOne(c => c.PatCountry).WithMany(c=> c.PatCountryLaws).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
        }
    }
}
