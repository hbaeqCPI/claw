using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatRelatedCasesMap : IEntityTypeConfiguration<PatRelatedCase>
    {
        public void Configure(EntityTypeBuilder<PatRelatedCase> builder)
        {
            builder.ToTable("tblPatAppRelatedCases");
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.RelatedCases).HasForeignKey(r => r.AppId).HasPrincipalKey(c => c.AppId);
        }
    }

    public class PatRelatedCasesDTOMap : IEntityTypeConfiguration<PatRelatedCaseDTO>
    {
        public void Configure(EntityTypeBuilder<PatRelatedCaseDTO> builder)
        {
            builder.ToView("vwPatRelatedCasesUnion");
            builder.HasOne(h => h.CountryApplication).WithMany(c => c.RelatedCasesDTO).HasForeignKey(r => r.AppId).HasPrincipalKey(c => c.AppId);
        }
    }
}
