using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QEMainMap : IEntityTypeConfiguration<QEMain>
    {
        public void Configure(EntityTypeBuilder<QEMain> builder)
        {
            builder.ToTable("tblQEMain");
            builder.HasIndex(qe => new { qe.ScreenId, qe.TemplateName }).IsUnique(); //unique index
            builder.HasOne(qe => qe.LanguageLookup).WithMany(l => l.LanguageQEMains).HasForeignKey(pa => pa.Language).HasPrincipalKey(l => l.LanguageName);
            builder.HasOne(qe => qe.SystemScreen).WithMany(s => s.QEsMain).HasForeignKey(qe => qe.ScreenId).HasPrincipalKey(s => s.ScreenId);
            builder.HasOne(qe => qe.DataSource).WithMany(d => d.QEsMain).HasForeignKey(qe => qe.DataSourceID).HasPrincipalKey(d => d.DataSourceID);
            builder.HasMany(qe => qe.LettersForSignature).WithOne(ls => ls.QEMain).HasForeignKey(ls => ls.SignatureQESetupId).HasPrincipalKey(qe => qe.QESetupID);  
            builder.HasOne(l => l.QECategory).WithMany(s => s.QEMains).HasForeignKey(l => l.QECatId).HasPrincipalKey(s => s.QECatId);

        }
    }
}



