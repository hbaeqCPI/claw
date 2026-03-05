using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data.Patent.mappings
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
