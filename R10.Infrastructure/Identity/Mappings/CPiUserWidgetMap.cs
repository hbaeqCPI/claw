using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserWidgetMap : IEntityTypeConfiguration<CPiUserWidget>
    {
        public void Configure(EntityTypeBuilder<CPiUserWidget> builder)
        {
            builder.ToTable("tblCPiUserWidgets");
            builder.HasKey(x => new { x.Id });
            builder.HasOne(uw => uw.CPiWidget).WithMany(w => w.CPiUserWidgets).HasForeignKey(x => x.WidgetId).HasPrincipalKey(x => x.Id).IsRequired(true);
        }
    }
}
