using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSCostExportLogMap : IEntityTypeConfiguration<AMSCostExportLog>
    {
        public void Configure(EntityTypeBuilder<AMSCostExportLog> builder)
        {
            builder.ToTable("tblAMSCostExportLog");
            builder.HasKey(l => l.LogID);
            builder.HasOne(l => l.AMSDue).WithOne(d => d.AMSCostExportLog).HasPrincipalKey<AMSDue>(d => d.DueID).HasForeignKey<AMSCostExportLog>(l => l.DueID);
        }
    }
}
