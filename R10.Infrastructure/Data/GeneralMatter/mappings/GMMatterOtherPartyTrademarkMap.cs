using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterOtherPartyTrademarkMap : IEntityTypeConfiguration<GMMatterOtherPartyTrademark>
    {
        public void Configure(EntityTypeBuilder<GMMatterOtherPartyTrademark> builder)
        {
            builder.ToTable("tblGMMatterOtherPartyTrademark");
            builder.HasKey(gp => gp.GMOPTId);
            builder.HasIndex(gopt => new { gopt.MatId, gopt.GMOPTId }).IsUnique();
            builder.HasOne(gopt => gopt.GMMatter).WithMany(gm => gm.OtherPartyTrademarks).HasForeignKey(gopt => gopt.MatId);
        }
    }
}
