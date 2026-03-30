using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAreaCountryMap : IEntityTypeConfiguration<PatAreaCountry>
    {
        public void Configure(EntityTypeBuilder<PatAreaCountry> builder)
        {
            builder.ToTable("tblPatAreaCountry");
            builder.HasKey(e => new { e.Area, e.Country });
        }
    }
}
