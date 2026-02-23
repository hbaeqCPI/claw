using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDiscussionMap : IEntityTypeConfiguration<DMSDiscussion>
    {
        public void Configure(EntityTypeBuilder<DMSDiscussion> builder)
        {
            builder.ToTable("tblDMSDiscussion");
            builder.HasIndex(d => new { d.DiscussId, d.DMSId }).IsUnique();
            builder.HasOne(d => d.Disclosure).WithMany(disc => disc.Discussions).HasForeignKey(d => d.DMSId).HasPrincipalKey(disc => disc.DMSId);            
        }
    }
}
