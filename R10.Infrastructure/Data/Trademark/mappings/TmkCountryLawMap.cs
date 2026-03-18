using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryLawMap : IEntityTypeConfiguration<TmkCountryLaw>
    {
        public void Configure(EntityTypeBuilder<TmkCountryLaw> builder)
        {
            builder.ToTable("tblTmkCountryLaw");
            // Unique constraint removed: duplicates allowed if Systems differ (overlap check done in code)
            builder.HasOne(cl => cl.TmkCaseType).WithMany(c => c.CaseTypeCountryLaws).HasPrincipalKey(c => c.CaseType).HasForeignKey(d => d.CaseType);
            builder.HasOne(cl => cl.TmkCountry).WithMany(c => c.TmkCountryLaws);
            // builder.HasOne(c => c.Agent).WithMany(c => c.AgentTmkCountryLaws).HasForeignKey(c => c.DefaultAgent); // Removed: Agent nav property no longer exists
            builder.HasMany(c => c.TmkCountryDues).WithOne(d => d.TmkCountryLaw)
                .HasPrincipalKey(c => c.CountryLawID).HasForeignKey(d => d.CountryLawID);
            builder.HasOne(c => c.TmkCountry).WithMany(c => c.TmkCountryLaws).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
        }
    }
}
