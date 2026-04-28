using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.DTOs;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatScoreDTOMap : IEntityTypeConfiguration<PatScoreDTO>
    {
        public void Configure(EntityTypeBuilder<PatScoreDTO> builder)
        {
            builder.ToView("vwPatScore");
        }
    }

    public class PatAverageScoreDTOMap : IEntityTypeConfiguration<PatAverageScoreDTO>
    {
        public void Configure(EntityTypeBuilder<PatAverageScoreDTO> builder)
        {
            builder.ToView("vwPatScoreAverage");
        }
    }
}
