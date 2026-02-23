using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadDataMap : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int AttributeId { get; set; }


        public MailDownloadDataAttribute? Attribute { get; set; }

        public List<MailDownloadDataMapPattern>? MapPatterns { get; set; }

        public List<MailDownloadActionFilter>? ActionFilters { get; set; }
    }
}
