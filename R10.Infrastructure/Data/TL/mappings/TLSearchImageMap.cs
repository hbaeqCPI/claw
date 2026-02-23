using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLSearchImageMap : IEntityTypeConfiguration<TLSearchImage>
    {
        public void Configure(EntityTypeBuilder<TLSearchImage> builder)
        {
            builder.ToTable("tblTLSearchImage");

        }
    }
}
