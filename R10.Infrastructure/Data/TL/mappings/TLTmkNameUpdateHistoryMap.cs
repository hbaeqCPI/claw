using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLTmkNameUpdateHistoryMap : IEntityTypeConfiguration<TLTmkNameUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<TLTmkNameUpdateHistory> builder)
        {
            builder.HasNoKey().ToView("vwTLUpdLogTrademarkName");
            
        }
    }
}
