using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiUserGroupMap : IEntityTypeConfiguration<CPiUserGroup>
    {
        public void Configure(EntityTypeBuilder<CPiUserGroup> builder)
        {
            builder.ToTable("tblCPiUserGroups");
            builder.HasKey(x => new { x.Id });
            builder.HasIndex(g => new { g.UserId, g.GroupId }).IsUnique();
            builder.HasOne(ug => ug.CPiUser).WithMany(u => u.CPiUserGroups).HasForeignKey(ug => ug.UserId).HasPrincipalKey(u => u.Id).IsRequired(true);
            builder.HasOne(ug => ug.CPiGroup).WithMany(g => g.CPiUserGroups).HasForeignKey(ug => ug.GroupId).HasPrincipalKey(g => g.Id).IsRequired(true);
        }
    }
}
