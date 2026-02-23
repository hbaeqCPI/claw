using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Data;
using R10.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace R10.Core.Services
{
    public class EFSService : IEFSService
    {
        private readonly IEFSRepository _repository;

        public EFSService(IEFSRepository repository)
        {
            _repository = repository;
        }


        public async Task<List<EFSFormDTO>> GetForms(string systemType, string docType, string country, int recId)
        {
            return await _repository.GetForms(systemType, docType, country, recId);
        }

        public async Task<DataSet> GetPrintData(string docType, string subType, string signatory, int recId, int pageNo, int noOfPages, string userId)
        {
          return await _repository.GetPrintData(docType, subType, signatory, recId, pageNo, noOfPages, userId);
        }

        public async Task<List<LookupDTO>> GetSignatories(string systemType, int recId)
        {
            return await _repository.GetSignatories(systemType,recId);
        }

        public async Task LogEFSDoc(string systemType, int efsDocId, string dataKey, int dataKeyValue,
            string efsFileName, string genBy, int pageNo, int pageCount, string? itemId, string? signatory)
        {
             await _repository.LogEFSDoc(systemType, efsDocId, dataKey, dataKeyValue, efsFileName, genBy, pageNo,
            pageCount,itemId, signatory);
        }

        public async Task UpdateEFS(IList<EFS> updated, string userName) { 
             await _repository.UpdateEFS(updated, userName);
        }

        public IQueryable<EFS> QueryableList => _repository.QueryableList;
    }
}
