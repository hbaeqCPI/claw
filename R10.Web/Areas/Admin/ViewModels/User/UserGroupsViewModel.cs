using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class UserGroupsViewModel : BaseEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [UIHint("UserGroup")]
        public PickListViewModel? Group { get; set; }
    }
}
