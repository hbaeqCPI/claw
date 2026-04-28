using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class OptionMap : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.ToTable("tblPubOptions");
            builder.HasIndex(o => new {o.OptionKey,o.OptionSubKey }).IsUnique();
        }
    }
}
