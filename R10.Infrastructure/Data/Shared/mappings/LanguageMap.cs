using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LanguageMap : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("tblLanguage");
            builder.Property(l => l.KeyID).ValueGeneratedOnAdd();
            builder.Property(l => l.KeyID).UseIdentityColumn();
            builder.Property(l => l.KeyID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(l => l.LanguageName).HasColumnName("Language");
        }
    }
}
