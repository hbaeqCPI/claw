using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
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
