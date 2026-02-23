using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIDSRelatedCasesDTOMap : IEntityTypeConfiguration<PatIDSRelatedCaseDTO>
    {
        public void Configure(EntityTypeBuilder<PatIDSRelatedCaseDTO> builder)
        {
            builder.ToView("vwPatIDSRelatedCasesUnion");
            builder.HasOne(h => h.CountryApplication).WithMany(c => c.IDSRelatedCasesDTO)
                .HasForeignKey(h => h.AppId).HasPrincipalKey(c => c.AppId);
        }
    }

    public class PatIDSRelatedCasesCopyDTOMap : IEntityTypeConfiguration<PatIDSRelatedCaseCopyDTO>
    {
        public void Configure(EntityTypeBuilder<PatIDSRelatedCaseCopyDTO> builder)
        {
            builder.ToView("vwPatIDSRelatedCasesCopyUnion");
        }
    }
}
