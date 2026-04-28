using LawPortal.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class UserGroupsViewModel : BaseEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [UIHint("UserGroup")]
        public PickListViewModel? Group { get; set; }
    }
}
