using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterOtherPartyPatentMap : IEntityTypeConfiguration<GMMatterOtherPartyPatent>
    {
        public void Configure(EntityTypeBuilder<GMMatterOtherPartyPatent> builder)
        {
            builder.ToTable("tblGMMatterOtherPartyPatent");
            builder.HasKey(gp => gp.GMOPPId);
            builder.HasIndex(gopt => new { gopt.MatId, gopt.GMOPPId }).IsUnique();
            builder.HasOne(gopt => gopt.GMMatter).WithMany(gm => gm.OtherPartyPatents).HasForeignKey(gopt => gopt.MatId);
        }
    }
}
