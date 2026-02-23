using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterRecordSourceFilterUserMap : IEntityTypeConfiguration<LetterRecordSourceFilterUser>
    {
        public void Configure(EntityTypeBuilder<LetterRecordSourceFilterUser> builder)
        {
            builder.ToTable("tblLetRecSourceFilterUser");
            builder.HasOne(f => f.LetterRecordSource).WithMany(r => r.LetterRecordSourceFiltersUser).HasForeignKey(f => f.RecSourceId).HasPrincipalKey(r => r.RecSourceId);
        }
    }
}
