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
    public class PatSearchDTOMap : IEntityTypeConfiguration<PatSearchDTO>
    {
        public void Configure(EntityTypeBuilder<PatSearchDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatSearchPrevResults");
            
        }
    }

    public class PatSearchExportDTOMap : IEntityTypeConfiguration<PatSearchExportDTO>
    {
        public void Configure(EntityTypeBuilder<PatSearchExportDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatSearchPrevResultsExport");

        }
    }

    public class PatSearchEmailDTOMap : IEntityTypeConfiguration<PatSearchEmailDTO>
    {
        public void Configure(EntityTypeBuilder<PatSearchEmailDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatSearchPrevResultsEmail");
        }
    }
}
