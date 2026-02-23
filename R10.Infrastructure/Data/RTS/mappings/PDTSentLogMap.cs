using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class PDTSentLogMap : IEntityTypeConfiguration<PDTSentLog>
    {
        public void Configure(EntityTypeBuilder<PDTSentLog> builder)
        {
            builder.ToTable("tblPDTSentLog");
        }
    }
}
