using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCaseTypeMap : IEntityTypeConfiguration<TmkCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkCaseType> builder)
        {
            builder.ToTable("tblTmkCaseType");
            builder.Property(c => c.CaseTypeId).ValueGeneratedOnAdd();
            builder.Property(c => c.CaseTypeId).UseIdentityColumn();
            builder.Property(c => c.CaseTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.CaseType).IsUnique();
            builder.Property(c => c.LockTmkRecord).HasDefaultValue(false);
        }
    }
}
