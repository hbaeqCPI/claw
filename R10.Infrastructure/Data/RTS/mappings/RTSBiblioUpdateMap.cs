using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class RTSBiblioUpdateMap : IEntityTypeConfiguration<RTSBiblioUpdate>
    {
        public void Configure(EntityTypeBuilder<RTSBiblioUpdate> builder)
        {
            builder.HasNoKey().ToView("vwPdtBiblioUpdate");

        }
    }
}
