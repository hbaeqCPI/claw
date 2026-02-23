using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcDiscussionMap : IEntityTypeConfiguration<TmcDiscussion>
    {
        public void Configure(EntityTypeBuilder<TmcDiscussion> builder)
        {
            builder.ToTable("tblTmcDiscussion");
            builder.HasIndex(d => new { d.DiscussId, d.TmcId }).IsUnique();
            builder.HasOne(d => d.Clearance).WithMany(disc => disc.Discussions).HasForeignKey(d => d.TmcId).HasPrincipalKey(c => c.TmcId);            
        }
    }
}
