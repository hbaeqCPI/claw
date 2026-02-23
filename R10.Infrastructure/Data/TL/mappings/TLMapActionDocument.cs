using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLMapActionDocumentMap : IEntityTypeConfiguration<TLMapActionDocument>
    {
        public void Configure(EntityTypeBuilder<TLMapActionDocument> builder)
        {
            builder.ToTable("tblTLMapActionDocument");

        }
    }

    public class TLMapActionDocumentClientMap : IEntityTypeConfiguration<TLMapActionDocumentClient>
    {
        public void Configure(EntityTypeBuilder<TLMapActionDocumentClient> builder)
        {
            builder.ToTable("tblTLMapActionDocumentClient");
            builder.HasOne(c => c.Client).WithMany(c=> c.TLMapActionDocumentClients);

        }
    }

    public class TLActionUpdateExcludeMap : IEntityTypeConfiguration<TLActionUpdateExclude>
    {
        public void Configure(EntityTypeBuilder<TLActionUpdateExclude> builder)
        {
            builder.ToTable("tblTLUpdActionExclude");

        }
    }

}
