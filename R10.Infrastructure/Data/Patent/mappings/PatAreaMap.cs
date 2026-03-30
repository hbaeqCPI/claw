using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAreaMap : IEntityTypeConfiguration<PatArea>
    {
        public void Configure(EntityTypeBuilder<PatArea> builder)
        {
            builder.ToTable("tblPatArea");
            builder.HasKey(e => new { e.Area, e.Systems });
            builder.Ignore(e => e.PatAreaCountries);
        }
    }
}
