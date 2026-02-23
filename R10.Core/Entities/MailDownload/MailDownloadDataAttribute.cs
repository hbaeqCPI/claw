using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadDataAttribute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 50,
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Length { get; set; }

        public bool ValueHasNoSpace { get; set; }


        public List<MailDownloadDataMap>? MailDownloadDataMaps { get; set; }
    }
}
