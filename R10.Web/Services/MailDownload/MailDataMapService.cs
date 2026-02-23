using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.MailDownload;
using R10.Core.Interfaces;
using System.Security.Claims;
using static iText.Kernel.Pdf.Colorspace.PdfSpecialCs;

namespace R10.Web.Services.MailDownload
{
    public class MailDataMapService : IMailDataMapService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;

        public MailDataMapService(ICPiDbContext cpiDbContext, ClaimsPrincipal user)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
        }

        public IQueryable<MailDownloadDataAttribute> Attributes => _cpiDbContext.GetRepository<MailDownloadDataAttribute>().QueryableList;

        public IQueryable<MailDownloadDataMap> Maps => _cpiDbContext.GetRepository<MailDownloadDataMap>().QueryableList;

        public IQueryable<MailDownloadDataMapPattern> MapPatterns => _cpiDbContext.GetRepository<MailDownloadDataMapPattern>().QueryableList;

        public async Task<MailDownloadDataMap> SaveMap(MailDownloadDataMap map)
        {
            var repository = _cpiDbContext.GetRepository<MailDownloadDataMap>();

            if (map.Id == 0)
                repository.Add(map);
            else
                repository.Update(map);

            await _cpiDbContext.SaveChangesAsync();
            return map;
        }

        public async Task RemoveMap(MailDownloadDataMap map)
        {
            _cpiDbContext.GetRepository<MailDownloadDataMap>().Delete(map);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateMapPatterns(int mapId, string userName, IEnumerable<MailDownloadDataMapPattern> updated, IEnumerable<MailDownloadDataMapPattern> added, IEnumerable<MailDownloadDataMapPattern> deleted)
        {
            var parent = await _cpiDbContext.GetRepository<MailDownloadDataMap>().GetByIdAsync(mapId);

            _cpiDbContext.GetRepository<MailDownloadDataMap>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.MapId = mapId;
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<MailDownloadDataMapPattern>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }
    }

    public interface IMailDataMapService
    {
        IQueryable<MailDownloadDataAttribute> Attributes { get; }
        IQueryable<MailDownloadDataMap> Maps { get; }
        IQueryable<MailDownloadDataMapPattern> MapPatterns { get; }
        Task<MailDownloadDataMap> SaveMap(MailDownloadDataMap map);
        Task RemoveMap(MailDownloadDataMap map);
        Task<bool> UpdateMapPatterns(int mapId, string userName,
            IEnumerable<MailDownloadDataMapPattern> updated,
            IEnumerable<MailDownloadDataMapPattern> added,
            IEnumerable<MailDownloadDataMapPattern> deleted);

    }
}
