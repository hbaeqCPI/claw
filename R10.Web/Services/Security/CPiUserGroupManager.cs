using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Services;
using System.Security.Claims;

namespace R10.Web.Services
{
    public class CPiUserGroupManager : BaseService<CPiGroup>, ICPiUserGroupManager
    {
        public CPiUserGroupManager(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public IQueryable<CPiGroup> GetGroups(string userId = "")
        {
            var groups = this.QueryableList.Where(g => g.IsEnabled);

            if (!string.IsNullOrEmpty(userId))
                groups = groups.Where(g => g.CPiUserGroups.Any(ug => ug.UserId == userId));

            return groups;
        }

        public IQueryable<CPiUser> GetUsers(int groupId = 0)
        {
            var users = _cpiDbContext.GetRepository<CPiUser>().QueryableList.Where(u => u.Status == CPiUserStatus.Approved);

            if (groupId > 0)
                users = users.Where(u => u.CPiUserGroups.Any(ug => ug.GroupId == groupId));

            return users;
        }

        public IQueryable<CPiUserGroup> CPiUserGroups => _cpiDbContext.GetRepository<CPiUserGroup>().QueryableList;

        public async Task<bool> UpdateGroupUser(int groupId, string userName, IEnumerable<CPiUserGroup> updated, IEnumerable<CPiUserGroup> added, IEnumerable<CPiUserGroup> deleted)
        {
            var parent = await _cpiDbContext.GetRepository<CPiGroup>().GetByIdAsync(groupId);

            //Guard.Against.NoRecordPermission(parent != null);

            _cpiDbContext.GetRepository<CPiGroup>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.GroupId = groupId;
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<CPiUserGroup>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserGroup(string userId, string userName, IEnumerable<CPiUserGroup> updated, IEnumerable<CPiUserGroup> added, IEnumerable<CPiUserGroup> deleted)
        {
            var parent = await _cpiDbContext.GetRepository<CPiUser>().GetByIdAsync(userId);

            //Guard.Against.NoRecordPermission(parent != null);

            _cpiDbContext.GetRepository<CPiUser>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.UserId = userId;
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<CPiUserGroup>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }
    }
}
