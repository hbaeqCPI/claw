using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkWorkflowActionParameterMap : IEntityTypeConfiguration<TmkWorkflowActionParameter>
    {
        public void Configure(EntityTypeBuilder<TmkWorkflowActionParameter> builder)
        {
            builder.ToTable("tblTmkWorkflowActionParameter");
          
        }
    }
}
