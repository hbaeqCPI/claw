using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    class TmkStandardGoodMap : IEntityTypeConfiguration<TmkStandardGood>
    {
        public void Configure(EntityTypeBuilder<TmkStandardGood> builder)
        {
            builder.ToTable("tblTmkStandardGood");
            builder.Property(sg => sg.ClassId).ValueGeneratedOnAdd();
            builder.HasIndex(sg => new { sg.Class, sg.ClassType }).IsUnique();
        }
    }

}
