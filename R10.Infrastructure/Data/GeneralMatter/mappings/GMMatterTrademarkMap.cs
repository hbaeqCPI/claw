using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterTrademarkMap : IEntityTypeConfiguration<GMMatterTrademark>
    {
        public void Configure(EntityTypeBuilder<GMMatterTrademark> builder)
        {
            builder.ToTable("tblGMMatterTrademark");
            builder.HasKey(gp => gp.GMTId);
            builder.HasIndex(gp => new { gp.MatId, gp.TmkId }).IsUnique();
            builder.HasOne(gp => gp.GMMatter).WithMany(gm => gm.Trademarks).HasForeignKey(gp => gp.MatId);
            builder.HasOne(gp => gp.TrademarkData).WithMany(t => t.GMMatterTrademarks).HasForeignKey(gp => gp.TmkId).HasPrincipalKey(t => t.TmkId);
        }
    }
}
