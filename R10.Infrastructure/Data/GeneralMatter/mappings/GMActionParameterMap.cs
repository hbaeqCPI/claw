using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMActionParameterMap : IEntityTypeConfiguration<GMActionParameter>
    {
        public void Configure(EntityTypeBuilder<GMActionParameter> builder)
        {
            builder.ToTable("tblGMActionParameter");
            builder.HasIndex(p => new { p.ActionTypeID, p.ActionDue, p.Yr, p.Mo, p.Dy }).IsUnique();
        }
    }
}
