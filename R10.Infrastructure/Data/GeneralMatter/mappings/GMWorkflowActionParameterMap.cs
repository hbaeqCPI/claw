using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMWorkflowActionParameterMap : IEntityTypeConfiguration<GMWorkflowActionParameter>
    {
        public void Configure(EntityTypeBuilder<GMWorkflowActionParameter> builder)
        {
            builder.ToTable("tblGMWorkflowActionParameter");
          
        }
    }
}
