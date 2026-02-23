using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatSubjectMatterMap : IEntityTypeConfiguration<PatSubjectMatter>
    {
        public void Configure(EntityTypeBuilder<PatSubjectMatter> builder)
        {
            builder.ToTable("tblPatSubjectMatter");
            
        }
    }
}
