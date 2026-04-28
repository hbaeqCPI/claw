using LawPortal.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Admin.ViewModels
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
