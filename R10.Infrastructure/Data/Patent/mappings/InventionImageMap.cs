using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class InventionImageMap : IEntityTypeConfiguration<InventionImage>
    {
        public void Configure(EntityTypeBuilder<InventionImage> builder)
        {
            builder.ToTable("vwPatImageInv");
            builder.HasOne(i => i.Invention).WithMany(i => i.Images).HasForeignKey(i => i.ParentId).HasPrincipalKey(i => i.InvId);
            builder.HasOne(i => i.DocType).WithMany(i => i.InventionImages).HasForeignKey(ti => ti.DocTypeId).HasPrincipalKey(t => t.DocTypeId);

        }
    }
}
