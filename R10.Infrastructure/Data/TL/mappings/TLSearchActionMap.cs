using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLSearchActionMap : IEntityTypeConfiguration<TLSearchAction>
    {
        public void Configure(EntityTypeBuilder<TLSearchAction> builder)
        {
            builder.ToTable("tblTLSearchAction");

        }
    }
}
