using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class EmailTypeMap : IEntityTypeConfiguration<EmailType>
    {
        public void Configure(EntityTypeBuilder<EmailType> builder)
        {
            builder.ToTable("tblEmailType");
            builder.HasKey(e => e.EmailTypeId);
            builder.HasIndex(e => e.Name).IsUnique();
            builder.HasOne(e => e.EmailTemplate).WithMany(t => t.EmailTypes);
            builder.HasOne(e => e.EmailContentType).WithMany(t => t.EmailTypes).HasForeignKey(t => t.ContentType).HasPrincipalKey(e => e.Name);
        }
    }
}
