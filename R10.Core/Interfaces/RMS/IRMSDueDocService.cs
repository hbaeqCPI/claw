using R10.Core.Entities.AMS;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.RMS
{
    public interface IRMSDueDocService : IBaseService<RMSDueDoc>
    {
        IQueryable<RMSDueDocUploadLog> UploadLogList { get; }

        Task<byte[]?> SaveUploaded(int ddId, int docId, string userName, bool isUploaded, int fileId, string fileName, byte[] tStamp, string reasonForChange = "");
        Task<byte[]?> SaveRequired(int ddId, int docId, string userName, bool isRequired, byte[] tStamp);
    }
}
