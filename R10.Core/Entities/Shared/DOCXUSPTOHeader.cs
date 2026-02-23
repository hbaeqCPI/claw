using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace R10.Core.Entities
{
    //public class DOCXUSPTOHeader: DOCXUSPTOHeaderDetail
    //{
    //    public List<DOCXUSPTOHeaderKeyword>? HeaderKeywords { get; set; }
    //}

    public class DOCXUSPTOHeader : BaseEntity
    {
        [Key]
        public int HId { get; set; }
        public string? HeaderName { get; set; }

    }
}
