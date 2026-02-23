using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatPriorityWebSvcMap : IEntityTypeConfiguration<PatPriorityWebSvc>
    {
        public void Configure(EntityTypeBuilder<PatPriorityWebSvc> builder)
        {
            builder.ToTable("tblPatPriorityWebSvc");
            builder.Property(i => i.EntityId).ValueGeneratedOnAdd();
            builder.Property(i => i.EntityId).UseIdentityColumn();
        }
    }
}
