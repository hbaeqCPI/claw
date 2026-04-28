using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Identity.Mappings
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
