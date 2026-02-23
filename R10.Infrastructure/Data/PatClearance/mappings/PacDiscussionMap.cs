using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacDiscussionMap : IEntityTypeConfiguration<PacDiscussion>
    {
        public void Configure(EntityTypeBuilder<PacDiscussion> builder)
        {
            builder.ToTable("tblPacDiscussion");
            builder.HasIndex(d => new { d.DiscussId, d.PacId }).IsUnique();
            builder.HasOne(d => d.Clearance).WithMany(disc => disc.Discussions).HasForeignKey(d => d.PacId).HasPrincipalKey(c => c.PacId);
        }
    }
}
