using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocFolderMap : IEntityTypeConfiguration<DocFolder>
    {
        public void Configure(EntityTypeBuilder<DocFolder> builder)
        {
            builder.ToTable("tblDocFolder");
            builder.HasMany(f => f.DocDocuments).WithOne(d => d.DocFolder).HasForeignKey(d=> d.FolderId).HasPrincipalKey(f=>f.FolderId).IsRequired(false);
            //builder.HasDiscriminator(d => new { d.SystemType, d.DataKey })
            //       .HasValue<TmkTrademarkDocFolder>(new { SystemType = "T", DataKey = "TmkId" });
        }
    }

    //public class TmkTrademarkDocFolderMap : IEntityTypeConfiguration<TmkTrademarkDocFolder>
    //{
    //    public void Configure(EntityTypeBuilder<TmkTrademarkDocFolder> builder)
    //    {
    //        builder.ToTable("tblDocFolder");
    //        builder.HasMany(f => f.DocDocuments).WithOne(d => d.DocFolder).HasForeignKey(d => d.FolderId).HasPrincipalKey(f => f.FolderId);
    //        //builder.HasDiscriminator(d => new { d.SystemType, d.DataKey })
    //        //       .HasValue<TmkTrademarkDocFolder>(new { SystemType = "T", DataKey = "TmkId" });
    //    }
    //}
}
