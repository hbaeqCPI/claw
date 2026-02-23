using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCaseTypeMap : IEntityTypeConfiguration<PatCaseType>
    {
        public void Configure(EntityTypeBuilder<PatCaseType> builder)
        {
            builder.ToTable("tblPatCaseType");
            builder.Property(s => s.CaseTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.CaseTypeId).UseIdentityColumn();
            builder.Property(m => m.CaseTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.CaseType).IsUnique();
            builder.HasMany(ct => ct.CaseTypeCountryApplication).WithOne(ca => ca.PatCaseType).HasForeignKey(ca => ca.CaseType).HasPrincipalKey(ct => ct.CaseType);


        }
    }
}
