using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacDiscussionReplyMap : IEntityTypeConfiguration<PacDiscussionReply>
    {
        public void Configure(EntityTypeBuilder<PacDiscussionReply> builder)
        {
            builder.ToTable("tblPacDiscussionReply");
            builder.HasIndex(d => new { d.ReplyId, d.DiscussId }).IsUnique();
            builder.HasOne(d => d.Discussion).WithMany(disc => disc.Replies).HasForeignKey(r => r.DiscussId).HasPrincipalKey(d => d.DiscussId);
        }
    }
}
