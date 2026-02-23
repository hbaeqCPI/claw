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
    public class PatDueDateWebSvcMap : IEntityTypeConfiguration<PatDueDateWebSvc>
    {
        public void Configure(EntityTypeBuilder<PatDueDateWebSvc> builder)
        {
            builder.ToTable("tblPatDueDateWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
            builder.HasOne(a => a.Action).WithMany(a => a.DueDates).HasForeignKey(a => a.ParentId).HasPrincipalKey(a => a.EntityId);
        }
    }
}
