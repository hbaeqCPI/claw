using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Documents;
using R10.Core.Entities.RMS;
using R10.Core.Interfaces;
using R10.Core.Interfaces.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.RMS
{
    public class RMSDueDocService : BaseService<RMSDueDoc>, IRMSDueDocService
    {
        public RMSDueDocService(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public IQueryable<RMSDueDocUploadLog> UploadLogList => _cpiDbContext.GetRepository<RMSDueDocUploadLog>().QueryableList;

        public async Task<byte[]?> SaveUploaded(int ddId, int docId, string userName, bool isUploaded, int fileId, string fileName, byte[] tStamp, string reasonForChange = "")
        {
            var dueDoc = await QueryableList.Where(d => d.DDId == ddId && d.DocId == docId).FirstOrDefaultAsync();
            var lastUpdate = DateTime.Now;
            var uploadLog = new RMSDueDocUploadLog()
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

                _cpiDbContext.GetRepository<RMSDueDoc>().Attach(dueDoc);
                dueDoc.IsUploaded = isUploaded;
                dueDoc.DateUploaded = lastUpdate;
                dueDoc.LastUpdate = lastUpdate;
                dueDoc.UpdatedBy = userName;
                dueDoc.RMSDueDocsUploadLogs = new List<RMSDueDocUploadLog>() { uploadLog };
            } 
            else
            {
                dueDoc = new RMSDueDoc()
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
                    RMSDueDocsUploadLogs = new List<RMSDueDocUploadLog>() { uploadLog }
            };
                _cpiDbContext.GetRepository<RMSDueDoc>().Add(dueDoc);
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
                _cpiDbContext.GetRepository<RMSDueDoc>().Attach(dueDoc);
                dueDoc.IsRequired = isRequired;
                dueDoc.LastUpdate = lastUpdate;
                dueDoc.UpdatedBy = userName;
            }
            else
            {
                dueDoc = new RMSDueDoc()
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
                _cpiDbContext.GetRepository<RMSDueDoc>().Add(dueDoc);
            }

            await _cpiDbContext.SaveChangesAsync();

            return dueDoc.tStamp;
        }
    }
}
