using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class EmailDataModelMap : IEntityTypeConfiguration<EmailContentType>
    {
        public void Configure(EntityTypeBuilder<EmailContentType> builder)
        {
            builder.ToTable("tblEmailContentType");
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.Id).UseIdentityColumn();
            builder.Property(e => e.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
