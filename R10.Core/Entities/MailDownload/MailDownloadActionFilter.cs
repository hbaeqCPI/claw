using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadActionFilter : BaseEntity
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ActionId { get; set; }

        [Required]
        public int MapId { get; set; }


        public MailDownloadAction? Action { get; set; }
        public MailDownloadDataMap? Map { get; set; }
    }
}
