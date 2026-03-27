using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaDeleteMap : IEntityTypeConfiguration<TmkAreaDelete>
    {
        public void Configure(EntityTypeBuilder<TmkAreaDelete> builder)
        {
            builder.ToTable("tblTmkAreaDelete");
            builder.HasNoKey();
        }
    }
}