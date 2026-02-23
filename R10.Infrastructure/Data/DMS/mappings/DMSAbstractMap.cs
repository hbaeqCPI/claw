using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSAbstractMap : IEntityTypeConfiguration<DMSAbstract>
    {
        public void Configure(EntityTypeBuilder<DMSAbstract> builder)
        {
            builder.ToTable("tblDMSAbstract");
            builder.HasIndex(da => new { da.AbstractId, da.DMSId }).IsUnique();            
            builder.HasOne(da => da.AbstractLanguage).WithMany(l => l.LanguageDMSAbstracts).HasForeignKey(da => da.Language).HasPrincipalKey(l => l.LanguageName);
        }
    }
}
