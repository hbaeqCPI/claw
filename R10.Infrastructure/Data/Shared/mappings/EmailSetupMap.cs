using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class EmailSetupMap : IEntityTypeConfiguration<EmailSetup>
    {
        public void Configure(EntityTypeBuilder<EmailSetup> builder)
        {
            builder.ToTable("tblEmailSetup");
            builder.HasKey(e => e.EmailSetupId);
            builder.HasIndex(e => new { e.EmailTypeId, e.Language }).IsUnique();
            builder.HasOne(e => e.LanguageLookup).WithMany(l => l.EmailSetups).HasForeignKey(e => e.Language).HasPrincipalKey(l => l.LanguageName);
            builder.HasOne(e => e.EmailType).WithMany(t => t.EmailSetups);
        }
    }
}
