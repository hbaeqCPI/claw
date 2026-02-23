using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterMap : IEntityTypeConfiguration<GMMatter>
    {
        public void Configure(EntityTypeBuilder<GMMatter> builder)
        {

            builder.ToTable("tblGMMatter");
            builder.HasKey(gm => gm.MatId);
            builder.HasIndex(gm => new { gm.CaseNumber, gm.SubCase }).IsUnique();
            builder.HasOne(ca => ca.ParentCase).WithMany(ca => ca.ChildCases).HasForeignKey(ca => ca.ParentMatId).HasPrincipalKey(ca => ca.MatId);
        }
    }
}
