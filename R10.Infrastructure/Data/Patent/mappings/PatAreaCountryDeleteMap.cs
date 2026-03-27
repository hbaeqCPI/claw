using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAreaCountryDeleteMap : IEntityTypeConfiguration<PatAreaCountryDelete>
    {
        public void Configure(EntityTypeBuilder<PatAreaCountryDelete> builder)
        {
            builder.ToTable("tblPatAreaCountryDelete");
            builder.HasNoKey();
        }
    }
}