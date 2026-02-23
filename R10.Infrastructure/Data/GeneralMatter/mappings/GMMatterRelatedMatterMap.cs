using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterRelatedMatterMap : IEntityTypeConfiguration<GMMatterRelatedMatter>
    {
        public void Configure(EntityTypeBuilder<GMMatterRelatedMatter> builder)
        {
            builder.ToTable("tblGMMatterRelatedMatter");
            builder.HasKey(gp => gp.GMMId);
            builder.HasOne(gp => gp.GMMatter).WithMany(gm => gm.RelatedMatters).HasForeignKey(gp => gp.MatId);
            builder.HasOne(gp => gp.RelatedGMMatter).WithMany(gm => gm.MatterRelateds).HasForeignKey(gp => gp.RelatedId).HasPrincipalKey(g => g.MatId);
            
        }
    }
}
