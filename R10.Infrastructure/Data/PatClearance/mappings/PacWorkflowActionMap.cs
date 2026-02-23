using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacWorkflowActionMap : IEntityTypeConfiguration<PacWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<PacWorkflowAction> builder)
        {
            builder.ToTable("tblPacWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();
        }
    }
}
