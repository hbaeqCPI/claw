using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocFixedFolderMap : IEntityTypeConfiguration<DocFixedFolder>
    {
        public void Configure(EntityTypeBuilder<DocFixedFolder> builder)
        {
            builder.ToTable("tblDocControlFixedFolder");
        }
    }
}
