using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    public class DocTreeEmailApiDTO : TreeBaseDTO
    {
        public new int id { get; set; }
        public int parentId { get; set; }
        public List<DocTreeEmailApiDTO> items { get; set; } = new List<DocTreeEmailApiDTO>() { };
    }

    //public class DocTreeEmailApiDTO
    //{
    //    public int id { get; set; }
    //    public int parentId { get; set; }
    //    public List<DocTreeEmailApiDTO> items { get; set; } = new List<DocTreeEmailApiDTO>() { };
    //    public string? text { get; set; }
    //    public bool hasChildren { get; set; }
    //    public bool expanded { get; set; }
    //}
}
