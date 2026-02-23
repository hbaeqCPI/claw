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
    public class PatAppImageMap : IEntityTypeConfiguration<PatAppImage>
    {
        public void Configure(EntityTypeBuilder<PatAppImage> builder)
        {
            builder.ToTable("vwPatImageApp");
            builder.HasOne(i => i.CountryApplication).WithMany(ca => ca.Images).HasForeignKey(i => i.ParentId).HasPrincipalKey(ca => ca.AppId);
            builder.HasOne(i => i.DocType).WithMany(d => d.PatAppImages).HasForeignKey(ti => ti.DocTypeId).HasPrincipalKey(t => t.DocTypeId);

        }
    }

    public class PatAppImageDefaultMap : IEntityTypeConfiguration<PatAppImageDefault>
    {
        public void Configure(EntityTypeBuilder<PatAppImageDefault> builder)
        {
            builder.ToTable("vwPatImageAppDefault");
            builder.HasOne(i => i.CountryApplication).WithOne(ca => ca.ImageDefault).HasForeignKey<PatAppImageDefault>(i => i.AppId);


        }
    }

}
