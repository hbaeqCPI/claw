using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLSearchTTABPartyMap : IEntityTypeConfiguration<TLSearchTTABParty>
    {
        public void Configure(EntityTypeBuilder<TLSearchTTABParty> builder)
        {
            builder.ToTable("tblTLSearchTTABParty");
        }
    }
}
