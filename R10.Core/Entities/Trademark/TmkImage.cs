using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class  TmkImage: ImageEntity
    {
        public TmkTrademark? TmkTrademark { get; set; }
        public DocType? DocType { get; set; }
    }
}
