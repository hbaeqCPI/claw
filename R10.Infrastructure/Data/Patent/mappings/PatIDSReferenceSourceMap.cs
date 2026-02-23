using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIDSReferenceSourceMap : IEntityTypeConfiguration<PatIDSReferenceSource>
    {
        public void Configure(EntityTypeBuilder<PatIDSReferenceSource> builder)
        {
            builder.ToTable("tblPatIDS_ReferenceSrc");
        }
    }
}
