using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchBiblioUSDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Class/Subclass")]
        public string? Class { get; set; }

        [Display(Name = "Examiner Name")]
        public string? Examiner { get; set; }

        [Display(Name = "Group Art Unit")]
        public string? GroupArtUnit { get; set; }

        [Display(Name = "Attorney Docket")]
        public string? AttorneyDocket { get; set; }

        [Display(Name = "Customer Number")]
        public string? CustomerNo { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "File Location")]
        public string? FileLocation { get; set; }

        [Display(Name = "Parent Case Text")]
        public string? ParentCaseText { get; set; }

        [Display(Name = "Notice")]
        public string? Notice { get; set; }

        [Display(Name = "Confirm No.")]
        public string? ConfirmNo { get; set; }
    }
}
