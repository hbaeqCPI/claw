using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterOtherPartyMap : IEntityTypeConfiguration<GMMatterOtherParty>
    {
        public void Configure(EntityTypeBuilder<GMMatterOtherParty> builder)
        {
            builder.ToTable("tblGMMatterOtherParty");
            builder.HasKey(go => go.OPID);
            builder.HasIndex(go => new { go.MatId, go.OtherParty, go.OtherPartyType }).IsUnique();
            builder.HasOne(go => go.GMMatter).WithMany(gm => gm.OtherParties).HasForeignKey(go => go.MatId);
            builder.HasOne(go => go.GMOtherParty).WithMany(op => op.GMMatterOtherParties).HasForeignKey(go => go.OtherParty);
            builder.HasOne(go => go.GMOtherPartyType).WithMany(opt => opt.GMMatterOtherParties).HasForeignKey(go => go.OtherPartyType);
        }
    }
}
