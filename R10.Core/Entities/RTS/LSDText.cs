using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class LSDText: LSDTextDetail
    {

    }

    public class LSDTextDetail
    {
        [Key]
        public int LSDTextID { get; set; }
        public int LSDID { get; set; }
        public string? FILEID { get; set; }
        public string? LETEXT { get; set; }
    }
}
