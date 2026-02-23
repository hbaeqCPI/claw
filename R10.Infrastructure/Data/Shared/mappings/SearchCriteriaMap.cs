using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class SearchCriteriaMap : IEntityTypeConfiguration<SearchCriteria>
    {
        public void Configure(EntityTypeBuilder<SearchCriteria> builder)
        {
            builder.ToTable("tblPubCriteria");
            builder.HasMany(c => c.CriteriaDetails).WithOne(cd => cd.SearchCriteria).HasForeignKey(cd => cd.CriteriaId).HasPrincipalKey(c => c.CriteriaId);
        }
    }

    public class SearchCriteriaDetailMap : IEntityTypeConfiguration<SearchCriteriaDetail>
    {
        public void Configure(EntityTypeBuilder<SearchCriteriaDetail> builder)
        {
            builder.ToTable("tblPubCriteria_Dtl");
            builder.HasOne(c => c.PatSearchNotify).WithOne(n => n.SearchCriteriaDetail).HasForeignKey<PatSearchNotify>(n => n.CritDtlId);

        }
    }
}
