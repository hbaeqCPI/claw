using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class RTSBiblioUpdateHistoryMap : IEntityTypeConfiguration<RTSBiblioUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<RTSBiblioUpdateHistory> builder)
        {
            builder.HasNoKey().ToView("vwPdtUpdLogBiblio");
            
        }
    }
}
