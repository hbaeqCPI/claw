using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIDSRelatedCasesMap : IEntityTypeConfiguration<PatIDSRelatedCase>
    {
        public void Configure(EntityTypeBuilder<PatIDSRelatedCase> builder)
        {
            builder.ToTable("tblPatIDSRelatedCases");
            builder.HasOne(r => r.CountryApplication).WithMany(c => c.IDSRelatedCases).HasForeignKey(r => r.AppId).HasPrincipalKey(c => c.AppId);

            //builder.HasOne(h => h.RelatedCountryApplication).WithMany(c => c.RelatedByCases)
            //    .HasForeignKey(r => r.RelatedAppId).HasPrincipalKey(c => c.AppId);

        }
    }
}
