using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class DeleteLogMap : IEntityTypeConfiguration<DeleteLog>
    {
        public void Configure(EntityTypeBuilder<DeleteLog> builder)
        {
            builder.ToTable("tblDeleteLog");
            builder.HasKey(d => d.DeleteLogId);            
        }
    }
}
