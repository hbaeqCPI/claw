using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;


namespace R10.Core.Interfaces
{
    
    public interface IEFSRepository 
    {

        Task<List<EFSFormDTO>> GetForms(string systemType, string docType, string country, int recId);

        Task<DataSet> GetPrintData(string docType, string subType, string signatory, int recId, int pageNo,
            int noOfPages, string userId);

        Task<List<LookupDTO>> GetSignatories(string systemType, int recId);

        Task LogEFSDoc(string systemType, int efsDocId, string dataKey, int dataKeyValue, string efsFileName,
            string genBy, int pageNo, int pageCount, string? itemId, string? signatory);

        Task UpdateEFS(IList<EFS> updated, string userName);

        IQueryable<EFS> QueryableList  { get; }
    }
}
