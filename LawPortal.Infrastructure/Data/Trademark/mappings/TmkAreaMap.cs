using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaMap : IEntityTypeConfiguration<TmkArea>
    {
        public void Configure(EntityTypeBuilder<TmkArea> builder)
        {
            builder.ToTable("tblTmkArea");
            builder.HasKey(e => new { e.Area, e.Systems });
            builder.Ignore(e => e.TmkAreaCountries);
        }
    }
}
