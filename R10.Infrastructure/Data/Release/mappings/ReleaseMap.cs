using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Release.mappings
{
    public class ReleaseMap : IEntityTypeConfiguration<R10.Core.Entities.Release>
    {
        public void Configure(EntityTypeBuilder<R10.Core.Entities.Release> builder)
        {
            builder.ToTable("tblRelease");
            builder.HasIndex(r => new { r.Year, r.Quarter, r.SystemType }).IsUnique();
        }
    }
}
