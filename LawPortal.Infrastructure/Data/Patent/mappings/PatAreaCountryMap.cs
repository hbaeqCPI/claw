using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatAreaCountryMap : IEntityTypeConfiguration<PatAreaCountry>
    {
        public void Configure(EntityTypeBuilder<PatAreaCountry> builder)
        {
            builder.ToTable("tblPatAreaCountry");
            builder.HasKey(e => new { e.Area, e.Country, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
