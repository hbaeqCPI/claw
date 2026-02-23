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
    public class PatCostEstimatorBaseAppDTOMap : IEntityTypeConfiguration<PatCostEstimatorBaseAppDTO>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorBaseAppDTO> builder)
        {
            builder.HasNoKey().ToView("vwPatCostEstimatorBaseApp");
            
        }
    }
}
