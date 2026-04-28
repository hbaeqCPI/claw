using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaDeleteMap : IEntityTypeConfiguration<TmkAreaDelete>
    {
        public void Configure(EntityTypeBuilder<TmkAreaDelete> builder)
        {
            builder.ToTable("tblTmkAreaDelete");
            builder.HasKey(e => new { e.Area, e.AreaNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}