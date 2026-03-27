using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryLawUpdateMap : IEntityTypeConfiguration<TmkCountryLawUpdate>
    {
        public void Configure(EntityTypeBuilder<TmkCountryLawUpdate> builder)
        {
            builder.ToTable("tblTmkCountryLawUpdate");
            builder.HasNoKey();
        }
    }
}
