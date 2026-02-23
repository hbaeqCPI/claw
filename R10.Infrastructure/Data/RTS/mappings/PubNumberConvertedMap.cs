using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PubNumberConvertedMap : IEntityTypeConfiguration<PubNumberConverted>
    {
        public void Configure(EntityTypeBuilder<PubNumberConverted> builder)
        {
            builder.ToTable("tblPubNumberConverted");

        }
}
    }
