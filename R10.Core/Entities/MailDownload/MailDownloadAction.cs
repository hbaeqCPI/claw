using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadAction : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public MailDownloadActionType ActionType { get; set; }


        public List<MailDownloadActionFilter>? ActionFilters { get; set; }
        public List<MailDownloadRule>? Rules { get; set; }
    }

    public enum MailDownloadActionType
    {
        None,
        Invention,
        CountryApplication,
        Trademark,
        GeneralMatter
    }
}
