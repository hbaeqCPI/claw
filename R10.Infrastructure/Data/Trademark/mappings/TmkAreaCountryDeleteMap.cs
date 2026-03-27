using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaCountryDeleteMap : IEntityTypeConfiguration<TmkAreaCountryDelete>
    {
        public void Configure(EntityTypeBuilder<TmkAreaCountryDelete> builder)
        {
            builder.ToTable("tblTmkAreaCountryDelete");
            builder.HasNoKey();
        }
    }
}