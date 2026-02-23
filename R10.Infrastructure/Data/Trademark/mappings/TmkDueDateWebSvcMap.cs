using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateWebSvcMap : IEntityTypeConfiguration<TmkDueDateWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateWebSvc> builder)
        {
            builder.ToTable("tblTmkDueDateWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
            builder.HasOne(a => a.Action).WithMany(a => a.DueDates).HasForeignKey(a => a.ParentId).HasPrincipalKey(a => a.EntityId);
        }
    }
}
