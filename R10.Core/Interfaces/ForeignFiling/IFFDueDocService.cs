using R10.Core.Entities.ForeignFiling;

namespace R10.Core.Interfaces.ForeignFiling
{
    public interface IFFDueDocService : IBaseService<FFDueDoc>
    {
        IQueryable<FFDueDocUploadLog> UploadLogList { get; }

        Task<byte[]?> SaveUploaded(int ddId, int docId, string userName, bool isUploaded, int fileId, string fileName, byte[] tStamp, string reasonForChange = "");
        Task<byte[]?> SaveRequired(int ddId, int docId, string userName, bool isRequired, byte[] tStamp);
    }
}
