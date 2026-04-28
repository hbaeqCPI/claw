using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiGroupMap : IEntityTypeConfiguration<CPiGroup>
    {
        public void Configure(EntityTypeBuilder<CPiGroup> builder)
        {
            builder.ToTable("tblCPiGroups");
            builder.HasKey(x => new { x.Id });
        }
    }
}
