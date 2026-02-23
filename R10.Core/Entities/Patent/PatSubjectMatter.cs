using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatSubjectMatter : BaseEntity
    {
        [Key]
        public int SubjectMatterId { get; set; }

        [Required]
        public int AppId { get; set; }

        [Required, StringLength(255)]
        public string? SubjectMatter { get; set; }

        public int OrderOfEntry { get; set; }

        public CountryApplication?  Application { get; set; }

    }
}
