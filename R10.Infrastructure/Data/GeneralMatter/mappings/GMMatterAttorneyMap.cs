using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterAttorneyMap : IEntityTypeConfiguration<GMMatterAttorney>
    {
        public void Configure(EntityTypeBuilder<GMMatterAttorney> builder)
        {
            builder.ToTable("tblGMMatterAttorney");
            builder.HasKey(ga => ga.AttID);
            builder.HasIndex(ga => new { ga.MatId, ga.AttorneyID }).IsUnique();
            builder.HasOne(ga => ga.GMMatter).WithMany(gm => gm.Attorneys).HasForeignKey(ga => ga.MatId);
            builder.HasOne(ga => ga.Attorney).WithMany(att => att.GMMatterAttorneys).HasForeignKey(ga => ga.AttorneyID);
        }
    }
}
