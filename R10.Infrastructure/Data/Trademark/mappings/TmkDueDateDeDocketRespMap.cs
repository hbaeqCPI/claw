using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkDueDateDeDocketRespMap : IEntityTypeConfiguration<TmkDueDateDeDocketResp>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateDeDocketResp> builder)
        {
            builder.ToTable("tblTmkDueDateDeDocketResp");
            builder.HasOne(a => a.TmkDueDateDeDocket).WithMany(c => c.TmkDueDateDeDocketResps).HasForeignKey(a => a.DeDocketId).HasPrincipalKey(c => c.DeDocketId);
        }
    }

    
}
