using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCaseTypeMap : IEntityTypeConfiguration<PatCaseType>
    {
        public void Configure(EntityTypeBuilder<PatCaseType> builder)
        {
            builder.ToTable("tblPatCaseType");
            builder.Property(s => s.CaseTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.CaseTypeId).UseIdentityColumn();
            builder.Property(m => m.CaseTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.CaseType).IsUnique();
        }
    }
}
