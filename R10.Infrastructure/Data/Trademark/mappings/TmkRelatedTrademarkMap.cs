using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkRelatedTrademarkMap : IEntityTypeConfiguration<TmkRelatedTrademark>
    {
        public void Configure(EntityTypeBuilder<TmkRelatedTrademark> builder)
        {
            builder.ToTable("tblTmkRelatedTrademark");
            builder.HasOne(r => r.Trademark).WithMany(tmk => tmk.TmkRelatedTrademarks).HasForeignKey(r => r.TmkId).HasPrincipalKey(t => t.TmkId);
            builder.HasOne(r => r.RelatedTrademark).WithMany(tmk => tmk.TmkTrademarkRelateds).HasForeignKey(r => r.RelatedTmkId).HasPrincipalKey(t => t.TmkId);
        }
    }
}
