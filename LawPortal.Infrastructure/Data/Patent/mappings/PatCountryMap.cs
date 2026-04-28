using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatCountryMap : IEntityTypeConfiguration<PatCountry>
    {
        public void Configure(EntityTypeBuilder<PatCountry> builder)
        {
            builder.ToTable("tblPatCountry");
            builder.HasKey(e => new { e.Country, e.Systems });
            builder.Ignore(e => e.PatCountryAreas);
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
