using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatAppImage : ImageEntity
    {
        public CountryApplication? CountryApplication { get; set; }
        public DocType? DocType { get; set; }
    }

    
    public class PatAppImageDefault
    {
        [Key]
        public int DocId { get; set; }
        public int AppId { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public string? ScreenCode { get; set; }
        public int ParentId { get; set; }
        public string? ImageFile { get; set; }
        public CountryApplication? CountryApplication { get; set; }

    }
}
