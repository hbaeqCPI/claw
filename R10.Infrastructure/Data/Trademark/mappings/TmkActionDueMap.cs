using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;


namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkActionDueMap : IEntityTypeConfiguration<TmkActionDue>
    {
        public void Configure(EntityTypeBuilder<TmkActionDue> builder)
        {
            builder.ToTable("tblTmkActionDue");
            builder.HasIndex(a => new { a.TmkId, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasIndex(a => new { a.CaseNumber, a.Country, a.SubCase, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasOne(a => a.TmkTrademark).WithMany(t => t.ActionDues).HasForeignKey(a => a.TmkId).HasPrincipalKey(c => c.TmkId);
            builder.HasOne(a => a.TmkCountry).WithMany(c => c.TmkActionsDue).HasForeignKey(a => a.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
