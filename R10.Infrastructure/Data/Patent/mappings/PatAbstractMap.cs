using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAbstractMap : IEntityTypeConfiguration<PatAbstract>
    {
        public void Configure(EntityTypeBuilder<PatAbstract> builder)
        {
            builder.ToTable("tblPatAbstract");
            builder.HasIndex(pa => new { pa.AbstractId, pa.InvId }).IsUnique();
            builder.Property(p => p.LanguageName).HasColumnName("Language");
            builder.HasOne(pa => pa.AbstractLanguage).WithMany(l => l.LanguagePatAbstracts).HasForeignKey(pa => pa.LanguageName).HasPrincipalKey(l => l.LanguageName);
            builder.HasOne(a => a.Invention).WithMany(i => i.Abstracts).HasForeignKey(a => a.InvId).HasPrincipalKey(i => i.InvId);

            builder.OwnsOne(pa => pa.TradeSecret, b => b.ToJson());
        }
    }
}
