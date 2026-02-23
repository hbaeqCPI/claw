using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterKeywordMap : IEntityTypeConfiguration<GMMatterKeyword>
    {
        public void Configure(EntityTypeBuilder<GMMatterKeyword> builder)
        {
            builder.ToTable("tblGMMatterKeyword");
            builder.HasKey(gk => gk.KwdId);
            builder.HasIndex(gk => new { gk.MatId, gk.Keyword }).IsUnique();
            builder.HasOne(gk => gk.GMMatter).WithMany(gm => gm.Keywords).HasForeignKey(gk => gk.MatId);
        }
    }
}
