using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaCountryMap : IEntityTypeConfiguration<TmkAreaCountry>
    {
        public void Configure(EntityTypeBuilder<TmkAreaCountry> builder)
        {
            builder.ToTable("tblTmkAreaCountry");
            builder.HasNoKey();
            builder.Ignore(e => e.Area);

            builder.Ignore(e => e.CreatedBy);
            builder.Ignore(e => e.UpdatedBy);
            builder.Ignore(e => e.DateCreated);
            builder.Ignore(e => e.LastUpdate);
        }
    }
}
