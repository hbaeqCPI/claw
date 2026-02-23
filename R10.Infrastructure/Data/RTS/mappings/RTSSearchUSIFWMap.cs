using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSSearchUSIFWMap : IEntityTypeConfiguration<RTSSearchUSIFW>
    {
        public void Configure(EntityTypeBuilder<RTSSearchUSIFW> builder)
        {
            builder.ToTable("tblPLSearchUSIFW").HasKey(c => new { c.PLAppID, c.OrderOfEntry });

        }
    }
}
