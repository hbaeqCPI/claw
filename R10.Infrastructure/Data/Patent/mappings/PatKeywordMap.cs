using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatKeywordMap : IEntityTypeConfiguration<PatKeyword>
    {
        public void Configure(EntityTypeBuilder<PatKeyword> builder)
        {
            builder.ToTable("tblPatKeyword");
            builder.HasIndex(pk => new { pk.KwdId, pk.InvId }).IsUnique();
            builder.HasIndex(pk => new { pk.Keyword }).IsUnique();
            builder.HasOne(k => k.Invention).WithMany(i => i.Keywords).HasForeignKey(i => i.InvId).HasPrincipalKey(i => i.InvId);
        }
    }
}
