using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWActionMapMap : IEntityTypeConfiguration<FormIFWActionMap>
    {
        public void Configure(EntityTypeBuilder<FormIFWActionMap> builder)
        {
            builder.ToTable("tblFRIFWActionMap");
        }
    }
}
