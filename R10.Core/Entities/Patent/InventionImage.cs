using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class InventionImage : ImageEntity
    {
        public Invention? Invention { get; set; }
        public DocType? DocType { get; set; }
    }
}
