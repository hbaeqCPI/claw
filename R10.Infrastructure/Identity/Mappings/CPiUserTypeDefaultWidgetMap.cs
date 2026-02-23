using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;

namespace R10.Infrastructure.Identity.Mappings
{
    internal class CPiUserTypeDefaultWidgetMap : IEntityTypeConfiguration<CPiUserTypeDefaultWidget>
    {
        public void Configure(EntityTypeBuilder<CPiUserTypeDefaultWidget> builder)
        {
            builder.ToTable("tblCPiUserTypeDefaultWidget");
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => new { u.UserType, u.WidgetId }).IsUnique(false);
            builder.HasOne(u => u.CPiWidget).WithMany(w => w.CPiUserTypeDefaultWidgets).HasForeignKey(x => x.WidgetId).HasPrincipalKey(x => x.Id).IsRequired(true);
        }
    }
}
