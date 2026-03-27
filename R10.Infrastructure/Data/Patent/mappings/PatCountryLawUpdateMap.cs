using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryLawUpdateMap : IEntityTypeConfiguration<PatCountryLawUpdate>
    {
        public void Configure(EntityTypeBuilder<PatCountryLawUpdate> builder)
        {
            builder.ToTable("tblPatCountryLawUpdate");
            builder.HasNoKey();
        }
    }
}
