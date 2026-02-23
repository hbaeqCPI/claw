using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryDueMap : IEntityTypeConfiguration<TmkCountryDue>
    {
        public void Configure(EntityTypeBuilder<TmkCountryDue> builder)
        {
            builder.ToTable("tblTmkCountryDue");
            //builder.HasIndex(c => new { c.Country, c.CaseType }).IsUnique();
            
        }
    }
}
