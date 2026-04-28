using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkActionParameterMap : IEntityTypeConfiguration<TmkActionParameter>
    {
        public void Configure(EntityTypeBuilder<TmkActionParameter> builder)
        {
            builder.ToTable("tblTmkActionParameter");
            builder.HasIndex(p => new { p.ActionTypeID, p.ActionDue, p.Yr, p.Mo, p.Dy }).IsUnique();
        }
    }
}
