using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data
{
    public class SignatureRepository : ISignatureRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        public SignatureRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DocReviewDTO>> GetDocs(string systemType,string displayType)
        {
            var list = await _dbContext.DocReviewDTO.FromSqlInterpolated($"procDoc_SignatureReview @SystemType = {systemType},@DisplayType={displayType}").AsNoTracking().ToListAsync();
            return list;

        }

        public async Task MarkReviewed(List<DocReviewDTO> updated,string userName)
        {
            var spUploadedDocsIds = updated.Where(u=> u.DocId.StartsWith("DS-")).Select(u => new DocReviewUpdateDTO { RecId= Convert.ToInt32(u.DocId.Split("-")[1]),SignatureReviewed=u.SignatureReviewed }).ToList();
            if (spUploadedDocsIds.Any()) {
                foreach (var spDoc in spUploadedDocsIds) {
                    if (spDoc.SignatureReviewed)
                    {
                        await _dbContext.SharePointFileSignatures.Where(f => f.SignatureFileId==spDoc.RecId).ExecuteUpdateAsync(f =>
                       f.SetProperty(p => p.SignatureReviewed, p => true)
                       .SetProperty(p => p.SignatureReviewedBy, p => userName)
                       .SetProperty(p => p.SignatureReviewedDate, p => DateTime.Now));
                    }
                    else {
                        await _dbContext.SharePointFileSignatures.Where(f => f.SignatureFileId == spDoc.RecId).ExecuteUpdateAsync(f =>
                           f.SetProperty(p => p.SignatureReviewed, p => false)
                           .SetProperty(p => p.SignatureReviewedBy, p => "")
                           .SetProperty(p => p.SignatureReviewedDate, p => null));
                    }
                }
            }

            var uploadedDocIds = updated.Where(u => u.DocId.StartsWith("D-")).Select(u => new DocReviewUpdateDTO { RecId = Convert.ToInt32(u.DocId.Split("-")[1]), SignatureReviewed = u.SignatureReviewed }).ToList();
            if (uploadedDocIds.Any())
            {
                foreach (var doc in uploadedDocIds)
                {
                    if (doc.SignatureReviewed)
                    {
                        await _dbContext.DocFileSignatures.Where(f => f.SignatureFileId == doc.RecId).ExecuteUpdateAsync(f =>
                       f.SetProperty(p => p.SignatureReviewed, p => true)
                       .SetProperty(p => p.SignatureReviewedBy, p => userName)
                       .SetProperty(p => p.SignatureReviewedDate, p => DateTime.Now));
                    }
                    else
                    {
                        await _dbContext.DocFileSignatures.Where(f => f.SignatureFileId == doc.RecId).ExecuteUpdateAsync(f =>
                           f.SetProperty(p => p.SignatureReviewed, p => false)
                           .SetProperty(p => p.SignatureReviewedBy, p => "")
                           .SetProperty(p => p.SignatureReviewedDate, p => null));
                    }
                }
            }

            var letterGenIds = updated.Where(u => u.DocId.StartsWith("L-")).Select(u => new DocReviewUpdateDTO { RecId = Convert.ToInt32(u.DocId.Split("-")[1]), SignatureReviewed = u.SignatureReviewed }).ToList();
            if (letterGenIds.Any())
            {
                foreach (var spDoc in letterGenIds)
                {
                    if (spDoc.SignatureReviewed)
                    {
                        await _dbContext.LetterLogs.Where(f => f.LetLogId == spDoc.RecId).ExecuteUpdateAsync(f =>
                       f.SetProperty(p => p.SignatureReviewed, p => true)
                       .SetProperty(p => p.SignatureReviewedBy, p => userName)
                       .SetProperty(p => p.SignatureReviewedDate, p => DateTime.Now));
                    }
                    else
                    {
                        await _dbContext.LetterLogs.Where(f => f.LetLogId == spDoc.RecId).ExecuteUpdateAsync(f =>
                           f.SetProperty(p => p.SignatureReviewed, p => false)
                           .SetProperty(p => p.SignatureReviewedBy, p => "")
                           .SetProperty(p => p.SignatureReviewedDate, p => null));
                    }
                }
            }

            var efsGenIds = updated.Where(u => u.DocId.StartsWith("E-")).Select(u => new DocReviewUpdateDTO { RecId = Convert.ToInt32(u.DocId.Split("-")[1]), SignatureReviewed = u.SignatureReviewed }).ToList();
            if (efsGenIds.Any())
            {
                foreach (var spDoc in efsGenIds)
                {
                    if (spDoc.SignatureReviewed)
                    {
                        await _dbContext.EFSLogs.Where(f => f.EfsLogId == spDoc.RecId).ExecuteUpdateAsync(f =>
                       f.SetProperty(p => p.SignatureReviewed, p => true)
                       .SetProperty(p => p.SignatureReviewedBy, p => userName)
                       .SetProperty(p => p.SignatureReviewedDate, p => DateTime.Now));
                    }
                    else
                    {
                        await _dbContext.EFSLogs.Where(f => f.EfsLogId == spDoc.RecId).ExecuteUpdateAsync(f =>
                           f.SetProperty(p => p.SignatureReviewed, p => false)
                           .SetProperty(p => p.SignatureReviewedBy, p => "")
                           .SetProperty(p => p.SignatureReviewedDate, p => null));
                    }
                }
            }

        }

    }
}
