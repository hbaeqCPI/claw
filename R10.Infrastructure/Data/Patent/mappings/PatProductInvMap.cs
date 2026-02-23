using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatProductInvMap : IEntityTypeConfiguration<PatProductInv>
    {
        public void Configure(EntityTypeBuilder<PatProductInv> builder)
        {
            builder.ToTable("tblPatProductInv");
            builder.HasOne(p => p.Invention).WithMany(p => p.Products).HasForeignKey(l => l.InvId).HasPrincipalKey(c => c.InvId);

        }
    }
}
