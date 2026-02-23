using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterPatentMap : IEntityTypeConfiguration<GMMatterPatent>
    {
        public void Configure(EntityTypeBuilder<GMMatterPatent> builder)
        {
            builder.ToTable("tblGMMatterPatent");
            builder.HasKey(gp => gp.GMPId);
            builder.HasIndex(gp => new { gp.MatId, gp.AppId, gp.InvId}).IsUnique();
            builder.HasOne(gp => gp.GMMatter).WithMany(gm => gm.Patents).HasForeignKey(gp => gp.MatId);
            builder.HasOne(gp => gp.ApplicationData).WithMany(a => a.GMMatterPatents).HasForeignKey(gp => gp.AppId).HasPrincipalKey(a => a.AppId);
            builder.HasOne(gp => gp.InventionData).WithMany(i => i.GMMatterPatents).HasForeignKey(gp => gp.InvId).HasPrincipalKey(i => i.InvId);
        }
    }
}
