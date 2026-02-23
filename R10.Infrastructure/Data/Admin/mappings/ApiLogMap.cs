using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data
{
    public class ApiLogMap : IEntityTypeConfiguration<ApiLog>
    {
        public void Configure(EntityTypeBuilder<ApiLog> builder)
        {
            builder.ToTable("ApiLogs");
        }
    }
}
