using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;

namespace R10.Core.Services.ForeignFiling
{
    public class FFDueDocService : BaseService<FFDueDoc>, IFFDueDocService
    {
        public FFDueDocService(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public IQueryable<FFDueDocUploadLog> UploadLogList => _cpiDbContext.GetRepository<FFDueDocUploadLog>().QueryableList;

        public async Task<byte[]?> SaveUploaded(int ddId, int docId, string userName, bool isUploaded, int fileId, string fileName, byte[] tStamp, string reasonForChange = "")
        {
            var dueDoc = await QueryableList.Where(d => d.DDId == ddId && d.DocId == docId).FirstOrDefaultAsync();
            var lastUpdate = DateTime.Now;
            var uploadLog = new FFDueDocUploadLog()
            {
                DocFileId = fileId,
                DocFileName = fileName,
                IsUploaded = isUploaded,
                ReasonForChange = reasonForChange,
                DateCreated = lastUpdate,
                CreatedBy = userName
            };

            if (dueDoc != null)
            {
                if (tStamp.Length > 0)
                    dueDoc.tStamp = tStamp;

                _cpiDbContext.GetRepository<FFDueDoc>().Attach(dueDoc);
                dueDoc.IsUploaded = isUploaded;
                dueDoc.DateUploaded = lastUpdate;
                dueDoc.LastUpdate = lastUpdate;
                dueDoc.UpdatedBy = userName;
                dueDoc.FFDueDocUploadLogs = new List<FFDueDocUploadLog>() { uploadLog };
            }
            else
            {
                dueDoc = new FFDueDoc()
                {
                    DDId = ddId,
                    DocId = docId,
                    IsRequired = true,
                    IsUploaded = isUploaded,
                    DateUploaded = lastUpdate,
                    LastUpdate = lastUpdate,
                    UpdatedBy = userName,
                    DateCreated = lastUpdate,
                    CreatedBy = userName,
                    FFDueDocUploadLogs = new List<FFDueDocUploadLog>() { uploadLog }
                };
                _cpiDbContext.GetRepository<FFDueDoc>().Add(dueDoc);
            }

            await _cpiDbContext.SaveChangesAsync();

            return dueDoc.tStamp;
        }

        public async Task<byte[]?> SaveRequired(int ddId, int docId, string userName, bool isRequired, byte[] tStamp)
        {
            var dueDoc = await QueryableList.Where(d => d.DDId == ddId && d.DocId == docId).FirstOrDefaultAsync();
            var lastUpdate = DateTime.Now;

            if (dueDoc != null)
            {
                dueDoc.tStamp = tStamp;
                _cpiDbContext.GetRepository<FFDueDoc>().Attach(dueDoc);
                dueDoc.IsRequired = isRequired;
                dueDoc.LastUpdate = lastUpdate;
                dueDoc.UpdatedBy = userName;
            }
            else
            {
                dueDoc = new FFDueDoc()
                {
                    DDId = ddId,
                    DocId = docId,
                    IsRequired = isRequired,
                    IsUploaded = false,
                    DateUploaded = lastUpdate,
                    LastUpdate = lastUpdate,
                    UpdatedBy = userName,
                    DateCreated = lastUpdate,
                    CreatedBy = userName
                };
                _cpiDbContext.GetRepository<FFDueDoc>().Add(dueDoc);
            }

            await _cpiDbContext.SaveChangesAsync();

            return dueDoc.tStamp;
        }
    }
}
