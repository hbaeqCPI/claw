using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSTaxSchedHistoryMap : IEntityTypeConfiguration<AMSTaxSchedHistory>
    {
        public void Configure(EntityTypeBuilder<AMSTaxSchedHistory> builder)
        {
            builder.ToTable("tblAMSTaxSchedHistory");
            builder.HasKey(l => l.LogID);
        }
    }
}
