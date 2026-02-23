using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSMapActionDocumentMap : IEntityTypeConfiguration<RTSMapActionDocument>
    {
        public void Configure(EntityTypeBuilder<RTSMapActionDocument> builder)
        {
            builder.ToTable("tblPLMapActionDocument");

        }
    }

    public class RTSMapActionDocumentClientMap : IEntityTypeConfiguration<RTSMapActionDocumentClient>
    {
        public void Configure(EntityTypeBuilder<RTSMapActionDocumentClient> builder)
        {
            builder.ToTable("tblPLMapActionDocumentClient");
            builder.HasOne(c => c.Client).WithMany(c=> c.RTSMapActionDocumentClients);

        }
    }
    
}
