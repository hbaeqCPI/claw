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
    public class FamilyTreeParentCaseDTOMap : IEntityTypeConfiguration<FamilyTreeParentCaseDTO>
    {
        public void Configure(EntityTypeBuilder<FamilyTreeParentCaseDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatCountryApplicationFamilyTreeParent");
        }
    }
}
