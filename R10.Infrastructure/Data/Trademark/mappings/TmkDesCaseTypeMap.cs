using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeMap : IEntityTypeConfiguration<TmkDesCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseType> builder)
        {
            builder.ToTable("tblTmkDesCaseType");
            builder.HasIndex(d => new { d.IntlCode, d.CaseType, d.DesCountry, d.DesCaseType }).IsUnique();
            builder.HasOne(d => d.ParentCountry).WithMany(c => c.ParentTmkDesCaseTypes).HasForeignKey(d => d.IntlCode).HasPrincipalKey(c => c.Country);
            builder.HasOne(d => d.ParentCaseType).WithMany(c => c.ParentTmkDesCaseTypes).HasForeignKey(d => d.CaseType).HasPrincipalKey(c => c.CaseType);
            builder.HasOne(d => d.ChildCountry).WithMany(c => c.ChildTmkDesCaseTypes).HasForeignKey(d => d.DesCountry).HasPrincipalKey(c => c.Country);
            builder.HasOne(d => d.ChildCaseType).WithMany(c => c.ChildTmkDesCaseTypes).HasForeignKey(d => d.DesCaseType).HasPrincipalKey(c => c.CaseType);
        }
    }
}
