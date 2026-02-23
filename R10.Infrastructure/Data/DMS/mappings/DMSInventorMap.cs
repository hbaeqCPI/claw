using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSInventorMap : IEntityTypeConfiguration<DMSInventor>
    {
        public void Configure(EntityTypeBuilder<DMSInventor> builder)
        {

            builder.ToTable("tblDMSInventor");
            builder.HasIndex(d => new { d.DMSId, d.InventorID }).IsUnique();
            builder.HasOne(di => di.InventorDMSDisclosure).WithMany(d=>d.Inventors).HasForeignKey(a => a.DMSId).HasPrincipalKey(i => i.DMSId);

        }
    }
}
