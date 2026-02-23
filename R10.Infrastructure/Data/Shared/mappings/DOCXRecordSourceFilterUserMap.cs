using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXRecordSourceFilterUserMap : IEntityTypeConfiguration<DOCXRecordSourceFilterUser>
    {
        public void Configure(EntityTypeBuilder<DOCXRecordSourceFilterUser> builder)
        {
            builder.ToTable("tblDOCXRecSourceFilterUser");
            builder.HasOne(f => f.DOCXRecordSource).WithMany(r => r.DOCXRecordSourceFiltersUser).HasForeignKey(f => f.RecSourceId).HasPrincipalKey(r => r.RecSourceId);
        }
    }
}
