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
    public class TmkAssignmentHistoryWebSvcMap : IEntityTypeConfiguration<TmkAssignmentHistoryWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkAssignmentHistoryWebSvc> builder)
        {
            builder.ToTable("tblTmkAssignmentHistoryWebSvc");
            builder.Property(i => i.EntityId).ValueGeneratedOnAdd();
            builder.Property(i => i.EntityId).UseIdentityColumn();
        }
    }
}
