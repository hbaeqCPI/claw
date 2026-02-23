using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLSearchMap : IEntityTypeConfiguration<TLSearch>
    {
        public void Configure(EntityTypeBuilder<TLSearch> builder)
        {
            builder.ToTable("tblTLSearch");
            builder.HasMany(s => s.TLSearchActions).WithOne(a => a.TLSearch).HasForeignKey(a => a.TLTmkId).HasPrincipalKey(s => s.TLTmkId);
            builder.HasOne(s => s.Trademark).WithOne(tmk => tmk.TLSearch);
            builder.HasMany(s => s.TLSearchImages).WithOne(a => a.TLSearch).HasForeignKey(a => a.TLTmkId).HasPrincipalKey(s => s.TLTmkId);
            builder.HasMany(s => s.TLSearchDocuments).WithOne(a => a.TLSearch).HasForeignKey(a => a.TLTmkId).HasPrincipalKey(s => s.TLTmkId);

        }
    }
}
