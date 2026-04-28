using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatAreaCountryDeleteMap : IEntityTypeConfiguration<PatAreaCountryDelete>
    {
        public void Configure(EntityTypeBuilder<PatAreaCountryDelete> builder)
        {
            builder.ToTable("tblPatAreaCountryDelete");
            builder.HasKey(e => new { e.Area, e.Country, e.AreaNew, e.CountryNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}