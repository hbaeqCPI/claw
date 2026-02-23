using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ClientContactMap : IEntityTypeConfiguration<ClientContact>
    {
        public void Configure(EntityTypeBuilder<ClientContact> builder)
        {
            builder.ToTable("tblClientContact");
            builder.HasIndex(cc => new { cc.ClientID, cc.ContactID }).IsUnique(); //unique index
            //builder.HasAlternateKey(cc => new { cc.ClientID, cc.ContactID }); //unique constraint (will not include the fields in Sql Update call)

        }
    }
}
