using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocIconMap : IEntityTypeConfiguration<DocIcon>
    {
        public void Configure(EntityTypeBuilder<DocIcon> builder)
        {
            builder.ToTable("tblDocIcon");
            builder.HasIndex(i => i.FileExt).IsUnique();
            builder.HasMany(i => i.DocFiles).WithOne(f => f.DocIcon).HasForeignKey(i => i.FileExt).HasPrincipalKey(f => f.FileExt);
        }
    }
}
