using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocTypeMap : IEntityTypeConfiguration<DocType>
    {
        public void Configure(EntityTypeBuilder<DocType> builder)
        {
            builder.ToTable("tblDocType");
        }
    }
}
