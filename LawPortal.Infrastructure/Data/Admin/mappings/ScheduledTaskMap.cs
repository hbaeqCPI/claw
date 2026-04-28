using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data
{
    public class ScheduledTaskMap : IEntityTypeConfiguration<ScheduledTask>
    {
        public void Configure(EntityTypeBuilder<ScheduledTask> builder)
        {
            builder.ToTable("tblCPiScheduledTasks");
            builder.HasKey(t => t.TaskId);
            builder.HasIndex(t => t.Name).IsUnique();
        }
    }
}
