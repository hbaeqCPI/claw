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
    public class AMSInstrxWebSvcMap : IEntityTypeConfiguration<AMSInstrxWebSvc>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxWebSvc> builder)
        {
            builder.ToTable("tblAMSInstrxWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
        }
    }
}
