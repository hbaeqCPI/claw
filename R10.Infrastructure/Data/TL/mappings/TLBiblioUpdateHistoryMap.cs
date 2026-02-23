using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLBiblioUpdateHistoryMap : IEntityTypeConfiguration<TLBiblioUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<TLBiblioUpdateHistory> builder)
        {
            builder.HasNoKey().ToView("vwTLUpdLogBiblio");
            
        }
    }
}
