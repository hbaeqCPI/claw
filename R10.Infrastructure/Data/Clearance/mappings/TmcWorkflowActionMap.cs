using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcWorkflowActionMap : IEntityTypeConfiguration<TmcWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<TmcWorkflowAction> builder)
        {
            builder.ToTable("tblTmcWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();
        }
    }
}
