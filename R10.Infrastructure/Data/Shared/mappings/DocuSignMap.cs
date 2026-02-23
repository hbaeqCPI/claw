using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DocuSignAnchorMap : IEntityTypeConfiguration<DocuSignAnchor>
    {
        public void Configure(EntityTypeBuilder<DocuSignAnchor> builder)
        {
            builder.ToTable("tblDocuSignAnchor");
            builder.HasMany(a => a.DocuSignAnchorTabs).WithOne(t => t.DocuSignAnchor).HasForeignKey(t => t.DocuSignAnchorId);
        }
    }

    public class DocuSignAnchorTabMap : IEntityTypeConfiguration<DocuSignAnchorTab>
    {
        public void Configure(EntityTypeBuilder<DocuSignAnchorTab> builder)
        {
            builder.ToTable("tblDocuSignAnchorTab");
            
        }
    }
}
