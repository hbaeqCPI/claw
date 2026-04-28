using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.mappings
{
    public class AppSystemMap : IEntityTypeConfiguration<AppSystem>
    {
        public void Configure(EntityTypeBuilder<AppSystem> builder)
        {
            builder.ToTable("tblCPiSystem");
            builder.Property(s => s.SystemId).ValueGeneratedOnAdd();
            builder.Property(s => s.SystemId).UseIdentityColumn();
            builder.Property(s => s.SystemId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.SystemName).IsUnique();
        }
    }
}
