using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Documents
{
    public class DocIcon
    {
        public int IconId { get; set; }

        [Key]
        public string? FileExt { get; set; }
        public string? IconClass { get; set; }

        public List<DocFile>? DocFiles { get; set; }

    }
}
