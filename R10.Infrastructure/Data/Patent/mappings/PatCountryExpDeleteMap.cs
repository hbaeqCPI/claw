using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryExpDeleteMap : IEntityTypeConfiguration<PatCountryExpDelete>
    {
        public void Configure(EntityTypeBuilder<PatCountryExpDelete> builder)
        {
            builder.ToTable("tblPatCountryExpDelete");
            builder.HasNoKey();
        }
    }
}