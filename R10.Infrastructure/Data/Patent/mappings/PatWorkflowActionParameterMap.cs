using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatWorkflowActionParameterMap : IEntityTypeConfiguration<PatWorkflowActionParameter>
    {
        public void Configure(EntityTypeBuilder<PatWorkflowActionParameter> builder)
        {
            builder.ToTable("tblPatWorkflowActionParameter");
          
        }
    }
}
