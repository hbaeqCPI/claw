using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatApplicationStatusMap : IEntityTypeConfiguration<PatApplicationStatus>
    {
        public void Configure(EntityTypeBuilder<PatApplicationStatus> builder)
        {
            builder.ToTable("tblPatApplicationStatus");
            builder.Property(s => s.StatusId).ValueGeneratedOnAdd();
            builder.Property(m => m.StatusId).UseIdentityColumn();
            builder.Property(m => m.StatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.ApplicationStatus).IsUnique();
        }
    }
}
