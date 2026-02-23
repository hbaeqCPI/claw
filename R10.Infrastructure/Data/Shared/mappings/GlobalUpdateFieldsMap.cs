using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class GlobalUpdateFieldsMap : IEntityTypeConfiguration<GlobalUpdateFields>
    {
        public void Configure(EntityTypeBuilder<GlobalUpdateFields> builder)
        {
            builder.ToTable("tblSysGlobalUpdate");
            builder.Property(gu => gu.FieldId).ValueGeneratedOnAdd();
            builder.HasIndex(gu => new { gu.SystemType, gu.UpdateField }).IsUnique();
        }
    }
}
