using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacKeywordMap : IEntityTypeConfiguration<PacKeyword>
    {
        public void Configure(EntityTypeBuilder<PacKeyword> builder)
        {
            builder.ToTable("tblPacKeyword");
            builder.HasIndex(pk => new { pk.KwdId, pk.PacId }).IsUnique();
            builder.HasIndex(pk => new { pk.Keyword }).IsUnique();
            builder.HasOne(k => k.Clearance).WithMany(t => t.Keywords).HasForeignKey(k => k.PacId).HasPrincipalKey(t => t.PacId);
        }
    }
}
