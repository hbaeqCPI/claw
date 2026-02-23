using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    class TLSearchTTABMap : IEntityTypeConfiguration<TLSearchTTAB>
    {
        public void Configure(EntityTypeBuilder<TLSearchTTAB> builder)
        {
            builder.ToTable("tblTLSearchTTAB");
        }
    }
}
