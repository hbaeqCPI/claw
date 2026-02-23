using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCaseTypeMap : IEntityTypeConfiguration<TmkCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkCaseType> builder)
        {
            builder.ToTable("tblTmkCaseType");
            builder.Property(c => c.CaseTypeId).ValueGeneratedOnAdd();
            builder.Property(c => c.CaseTypeId).UseIdentityColumn();
            builder.Property(c => c.CaseTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.CaseType).IsUnique();
            builder.Property(c => c.LockTmkRecord).HasDefaultValue(false);
            builder.HasMany(c => c.CaseTypeTrademark).WithOne(c => c.TmkCaseType).HasForeignKey(t => t.CaseType).HasPrincipalKey(t => t.CaseType);
        }
    }
}
