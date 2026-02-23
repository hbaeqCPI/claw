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
    public class PatParentCaseDTOMap : IEntityTypeConfiguration<PatParentCaseDTO>
    {
        public void Configure(EntityTypeBuilder<PatParentCaseDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatCountryApplicationParent");
            
        }
    }

    public class PatParentCaseTDDTOMap : IEntityTypeConfiguration<PatParentCaseTDDTO>
    {
        public void Configure(EntityTypeBuilder<PatParentCaseTDDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatCountryApplicationParentTD");

        }
    }
}
