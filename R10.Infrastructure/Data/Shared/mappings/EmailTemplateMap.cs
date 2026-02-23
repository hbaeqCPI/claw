using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class EmailTemplateMap : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> builder)
        {
            builder.ToTable("tblEmailTemplate");
            builder.HasKey(e => e.EmailTemplateId);
            builder.HasIndex(e => e.Name).IsUnique();
        }
    }
}
