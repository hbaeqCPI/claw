using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Release.mappings
{
    public class DeployLogMap : IEntityTypeConfiguration<DeployLog>
    {
        public void Configure(EntityTypeBuilder<DeployLog> builder)
        {
            builder.ToTable("tblDeployLog");
            builder.Property(d => d.DeployLogId).ValueGeneratedOnAdd().UseIdentityColumn();
        }
    }
}
