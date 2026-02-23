using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDiscussionReplyMap : IEntityTypeConfiguration<DMSDiscussionReply>
    {
        public void Configure(EntityTypeBuilder<DMSDiscussionReply> builder)
        {
            builder.ToTable("tblDMSDiscussionReply");
            builder.HasIndex(d => new { d.ReplyId, d.DiscussId }).IsUnique();
            builder.HasOne(k => k.Discussion).WithMany(d => d.Replies).HasForeignKey(k => k.DiscussId).HasPrincipalKey(d => d.DiscussId);
        }
    }
}
