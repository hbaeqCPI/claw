using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
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
