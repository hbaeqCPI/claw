using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxDecisionMgtMap : IEntityTypeConfiguration<AMSInstrxDecisionMgt>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxDecisionMgt> builder)
        {
            builder.ToTable("tblAMSInstrxDecisionMgt");
            builder.HasKey(d => d.DecisionMgtID);
            builder.HasOne(d => d.AMSDue).WithMany(due => due.AMSInstrxDecisionMgt).HasForeignKey(d => d.DueID).HasPrincipalKey(due => due.DueID);
        }
    }
}
