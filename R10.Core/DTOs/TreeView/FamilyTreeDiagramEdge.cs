using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    public class FamilyTreeDiagramEdge
    {
        public string StartId { get; set; } = "";
        public string EndId { get; set; } = "";
        public string? Label { get; set; }
    }
}
