using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeMap : IEntityTypeConfiguration<PatDesCaseType>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseType> builder)
        {
            builder.ToTable("tblPatDesCaseType");
            builder.HasIndex(d => new { d.IntlCode, d.CaseType, d.DesCountry, d.DesCaseType }).IsUnique();
            builder.HasOne(d => d.ParentCountry).WithMany(c => c.ParentPatDesCaseTypes).HasForeignKey(d => d.IntlCode).HasPrincipalKey(c => c.Country);
            builder.HasOne(d => d.ParentCaseType).WithMany(c => c.ParentPatDesCaseTypes).HasForeignKey(d => d.CaseType).HasPrincipalKey(c => c.CaseType);
            builder.HasOne(d => d.ChildCountry).WithMany(c => c.ChildPatDesCaseTypes).HasForeignKey(d => d.DesCountry).HasPrincipalKey(c => c.Country);
            builder.HasOne(d => d.ChildCaseType).WithMany(c => c.ChildPatDesCaseTypes).HasForeignKey(d => d.DesCaseType).HasPrincipalKey(c => c.CaseType);
            builder.HasOne(d => d.PatCountryLaw).WithMany(c => c.PatDesCaseTypes).HasForeignKey(d => new { d.IntlCode, d.CaseType }).HasPrincipalKey(c => new { c.Country, c.CaseType });
        }
    }
}
