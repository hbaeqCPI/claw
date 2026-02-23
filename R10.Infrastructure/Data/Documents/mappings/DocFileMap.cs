using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;


namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocFileMap : IEntityTypeConfiguration<DocFile>
    {
        public void Configure(EntityTypeBuilder<DocFile> builder)
        {
            builder.ToTable("tblDocFile");
            //builder.Property(df => df.FileId).ValueGeneratedOnAdd();
            builder.Property(df => df.DocFileName).HasComputedColumnSql("(CONVERT([varchar],[FileId])+ '.' +[DocFileExt])");
            builder.Property(df => df.ThumbFileName).HasComputedColumnSql("(case [IsImage] when (0) then '' else (CONVERT([varchar],[FileId])+'_thumb.')+[DocFileExt] end)");
            
        }
    }
}
