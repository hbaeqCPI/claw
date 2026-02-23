using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLSearchDocumentMap : IEntityTypeConfiguration<TLSearchDocument>
    {
        public void Configure(EntityTypeBuilder<TLSearchDocument> builder)
        {
            builder.ToTable("tblTLSearchDocuments");

        }
    }
}
