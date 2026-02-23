using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSMapActionDueMap : IEntityTypeConfiguration<RTSMapActionDue>
    {
        public void Configure(EntityTypeBuilder<RTSMapActionDue> builder)
        {
            builder.ToTable("tblPLMapActionDue");
        }
    }

    public class RTSMapActionDueSourceMap : IEntityTypeConfiguration<RTSMapActionDueSource>
    {
        public void Configure(EntityTypeBuilder<RTSMapActionDueSource> builder)
        {
            builder.ToTable("tblPLMapActionDueSource");
            builder.HasMany(c => c.ActionsClose).WithOne(a => a.ActionSource).HasForeignKey(c => c.CloseSourceId);
        }
    }

    public class RTSMapActionCloseMap : IEntityTypeConfiguration<RTSMapActionClose>
    {
        public void Configure(EntityTypeBuilder<RTSMapActionClose> builder)
        {
            builder.ToTable("tblPLMapActionClose");
        }
    }
}
