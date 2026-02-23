using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class PatParentCaseDTO
    {
        public string? ParentCase { get; set; }
        public int ParentId { get; set; }
        public string? PatNumber { get; set; }
    }

    //will not work with EF mapping 
    //public class PatParentCaseTDDTO:PatParentCaseDTO
    //{
    //}

    public class PatParentCaseTDDTO
    {
        public string? ParentCase { get; set; }
        public int ParentId { get; set; }
        public string? PatNumber { get; set; }
    }
}
