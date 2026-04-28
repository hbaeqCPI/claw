using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Release.mappings
{
    public class ReleaseMap : IEntityTypeConfiguration<LawPortal.Core.Entities.Release>
    {
        public void Configure(EntityTypeBuilder<LawPortal.Core.Entities.Release> builder)
        {
            builder.ToTable("tblRelease");
            builder.HasIndex(r => new { r.Year, r.Quarter, r.SystemType }).IsUnique();
        }
    }
}
