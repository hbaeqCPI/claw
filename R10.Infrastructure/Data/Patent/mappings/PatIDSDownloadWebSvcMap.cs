using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIDSDownloadWebSvcMap : IEntityTypeConfiguration<PatIDSDownloadWebSvc>
    {
        public void Configure(EntityTypeBuilder<PatIDSDownloadWebSvc> builder)
        {

            builder.ToTable("tblPatIDSDownloadWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
        }
    }
}