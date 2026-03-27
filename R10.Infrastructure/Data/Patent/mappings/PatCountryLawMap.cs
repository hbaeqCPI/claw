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
            builder.HasKey(e => new { e.Country, e.CaseType, e.Systems });
            builder.Ignore(e => e.CopyOptions);
            builder.Ignore(e => e.PatCountryDues);
        }
    }
}
