using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Identity.Mappings
{
    public class LocalizationRecordsMap : IEntityTypeConfiguration<LocalizationRecords>
    {
        public void Configure(EntityTypeBuilder<LocalizationRecords> builder)
        {
            builder.ToTable("LocalizationRecords");
            builder.HasKey(x => new { x.Id });
        }
    }
}
