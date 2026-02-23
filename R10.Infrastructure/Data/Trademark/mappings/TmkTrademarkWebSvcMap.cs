using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkWebSvcMap : IEntityTypeConfiguration<TmkTrademarkWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkTrademarkWebSvc> builder)
        {

            builder.ToTable("tblTmkTrademarkWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn(); 
        }
    }
}