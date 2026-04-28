using LawPortal.Core.Identity;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserGroupManager : IBaseService<CPiGroup>
    {
        IQueryable<CPiUser> GetUsers(int groupId = 0);
        IQueryable<CPiGroup> GetGroups(string userId = "");

        IQueryable<CPiUserGroup> CPiUserGroups { get; }

        Task<bool> UpdateGroupUser(int groupId, string userName,
            IEnumerable<CPiUserGroup> updated,
            IEnumerable<CPiUserGroup> added,
            IEnumerable<CPiUserGroup> deleted);

        Task<bool> UpdateUserGroup(string userId, string userName,
            IEnumerable<CPiUserGroup> updated,
            IEnumerable<CPiUserGroup> added,
            IEnumerable<CPiUserGroup> deleted);
    }
}
