using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiWidgetMap : IEntityTypeConfiguration<CPiWidget>
    {
        public void Configure(EntityTypeBuilder<CPiWidget> builder)
        {
            builder.ToTable("tblCPiWidgets");
            builder.HasKey(x => new { x.Id });
            builder.HasMany(w => w.CPiUserWidgets).WithOne(uw => uw.CPiWidget).HasForeignKey(x => x.WidgetId).HasPrincipalKey(x => x.Id).IsRequired(true);
        }
    }
}
