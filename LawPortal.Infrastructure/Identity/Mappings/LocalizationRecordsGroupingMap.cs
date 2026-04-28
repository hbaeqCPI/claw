using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class LocalizationRecordsGroupingMap : IEntityTypeConfiguration<LocalizationRecordsGrouping>
    {
        public void Configure(EntityTypeBuilder<LocalizationRecordsGrouping> builder)
        {
            builder.ToTable("LocalizationRecords_Grouping");
            builder.HasKey(x => new { x.Id });
            //builder.HasMany<LocalizationRecords>().WithOne(localization => localization.Group).HasPrincipalKey(group => group.ResourceKey);
            builder.HasMany(g => g.LocalizationRecords).WithOne(l => l.Group).HasPrincipalKey(g => g.ResourceKey).HasForeignKey(l => l.ResourceKey);
        }
    }
}
