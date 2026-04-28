using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiDefaultPageMap : IEntityTypeConfiguration<CPiDefaultPage>
    {
        public void Configure(EntityTypeBuilder<CPiDefaultPage> builder)
        {
            builder.ToTable("tblCPiDefaultPages");
            builder.HasKey(x => new { x.Id });
        }
    }
}
