using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXUSPTOHeaderKeywordMap : IEntityTypeConfiguration<DOCXUSPTOHeaderKeyword>
    {
        public void Configure(EntityTypeBuilder<DOCXUSPTOHeaderKeyword> builder)
        {
            builder.ToTable("tblDOCXUSPTOHeaderKeyword");
            builder.HasKey("KId");
            //builder.Property(k => k.KId).HasColumnName("KId").IsRequired();
            //builder.HasOne(k => k.Header).WithMany(h => h.HeaderKeywords).HasForeignKey(h => h.HId).HasPrincipalKey(h => h.HId);
        }
    }   
}
