using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class GroupUsersViewModel : BaseEntity
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        [UIHint("GroupUser")]
        public PickListViewModel? User { get; set; }

        public string? Email { get; set; }
    }
}
