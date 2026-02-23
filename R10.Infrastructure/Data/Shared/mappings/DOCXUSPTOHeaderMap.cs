using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXUSPTOHeaderMap : IEntityTypeConfiguration<DOCXUSPTOHeader>
    {
        public void Configure(EntityTypeBuilder<DOCXUSPTOHeader> builder)
        {
            builder.ToTable("tblDOCXUSPTOHeader");
            builder.HasKey("HId");
            //builder.Property(h => h.HId).HasColumnName("HId").IsRequired();
        }
    }
}
