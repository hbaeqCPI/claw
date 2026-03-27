using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryMap : IEntityTypeConfiguration<PatCountry>
    {
        public void Configure(EntityTypeBuilder<PatCountry> builder)
        {
            builder.ToTable("tblPatCountry");
            builder.HasKey(e => e.Country);
            builder.Ignore(e => e.PatCountryAreas);
        }
    }
}
