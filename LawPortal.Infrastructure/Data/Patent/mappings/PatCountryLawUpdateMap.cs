using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
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
