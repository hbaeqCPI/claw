using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSKeywordMap : IEntityTypeConfiguration<DMSKeyword>
    {
        public void Configure(EntityTypeBuilder<DMSKeyword> builder)
        {
            builder.ToTable("tblDMSKeyword");
            //builder.HasIndex(pk => new { pk.KwdId, pk.DMSId }).IsUnique();
            //builder.HasIndex(pk => new { pk.Keyword }).IsUnique();

            builder.HasIndex(k => new { k.DMSId, k.Keyword }).IsUnique();
            builder.HasOne(k => k.Disclosure).WithMany(d => d.Keywords).HasForeignKey(k => k.DMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
