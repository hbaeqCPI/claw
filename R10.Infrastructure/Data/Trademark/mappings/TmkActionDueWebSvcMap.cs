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
    public class TmkActionDueWebSvcMap : IEntityTypeConfiguration<TmkActionDueWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkActionDueWebSvc> builder)
        {
            builder.ToTable("tblTmkActionDueWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
        }
    }
}
