using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaMap : IEntityTypeConfiguration<TmkArea>
    {
        public void Configure(EntityTypeBuilder<TmkArea> builder)
        {
            builder.ToTable("tblTmkArea");
            builder.HasNoKey();
            builder.Ignore(e => e.TmkAreaCountries);
        }
    }
}
