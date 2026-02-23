using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkMarkTypeMap : IEntityTypeConfiguration<TmkMarkType>
    {
        public void Configure(EntityTypeBuilder<TmkMarkType> builder)
        {
            builder.ToTable("tblTmkMarkType");
            builder.HasAlternateKey(i => i.MarkType);
            builder.Property(m => m.MarkTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.MarkTypeId).UseIdentityColumn();
            builder.Property(m => m.MarkTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
