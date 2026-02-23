using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data
{

    public class TLInfoRepository:ITLInfoRepository 
    {
        private readonly ApplicationDbContext _dbContext;
        public TLInfoRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TLInfoSettingsMenu>> GetMenu(string country)
        {
            var menu = await _dbContext.TLInfoCountrySettings.Where(m => m.Country == country || m.Country == "ALL")
                .OrderBy(m => m.Sequence).Select(m => m.InfoSettingsMenu).ToListAsync();
            return menu;
        }

        public TLSearchBiblioDTO GetBiblio(int tlTmkId)
        {
            var biblio =  _dbContext.TLSearchBiblioDTO.FromSqlInterpolated($"procTLPTO_CaseInfo @TLTmkId={tlTmkId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return biblio;
        }

        public TLSearchImageDTO GetImage(int tlTmkId)
        {
            var image = _dbContext.TLSearchImageDTO.FromSqlInterpolated($"procTLPTO_Image @TLTmkId={tlTmkId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return image;
        }

        public async Task<List<TLSearchAssignmentDTO>> GetAssignments(int tlTmkId)
        {
            var assignments = await _dbContext.TLSearchAssignmentDTO.FromSqlInterpolated($"procTLPTO_Assignment @TLTmkId={tlTmkId}").AsNoTracking().ToListAsync();
            return assignments;
        }

        public async Task<List<TLSearchGoodsDTO>> GetGoods(int tlTmkId)
        {
            var goods = await _dbContext.TLSearchGoodsDTO.FromSqlInterpolated($"procTLPTO_Goods @TLTmkId={tlTmkId}").AsNoTracking().ToListAsync();
            return goods;
        }

        public async Task<List<TLSearchActionAsDownloadedDTO>> GetActions(int tlTmkId, bool asDownloaded)
        {
            var actions = await _dbContext.TLSearchActionAsDownloadedDTO.FromSqlInterpolated($"procTLPTO_Actions @TLTmkId={tlTmkId},@Action={(asDownloaded ? 1 : 2)}").AsNoTracking().ToListAsync();
            return actions;
        }

        public async Task<List<TLSearchDocDTO>> GetDocuments(int tlTmkId)
        {
            var docs = await _dbContext.TLSearchDocDTO.FromSqlInterpolated($"procTLPTO_Docs @TLTmkId={tlTmkId}").AsNoTracking().ToListAsync();
            return docs;
        }

        public async Task<List<TLSearchTTABDTO>> GetTTABs(int tlTmkId) {
            var ttabs = await _dbContext.TLSearchTTABDTO.FromSqlInterpolated($"procTLPTO_TTABs @TLTmkId={tlTmkId}").AsNoTracking().ToListAsync();
            return ttabs;
        }

        public async Task<bool> HasTL(string country)
        {
            var parameters = SqlHelper.BuildSqlParameters(new { Country = country });
            parameters.Add(new SqlParameter
            {
                ParameterName = "Result",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            });
            
            await _dbContext.Database.ExecuteSqlRawAsync("exec procTLPTO_HasTL @Country, @Result Out", parameters);
            var result = (int)parameters[1].Value;
            return result > 0;
        }

        public async Task ClearPTOData(int tlTmkId)
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"exec procTLPTO_RecordClear @TLTmkId={tlTmkId}");
        }

        public void MarkDocAsTransferred(string fileName)
        {
            var sql = "Update tblTLSearchDocuments Set Transferred=1 Where FileName=@FileName";
            var param = new SqlParameter("@FileName", fileName);
            _dbContext.Database.ExecuteSqlRaw(sql, param);
        }

        public void MarkImageAsTransferred(string fileName)
        {
            var sql = "Update tblTLSearchImage Set Transferred=1 Where OrigFileName=@FileName";
            var param = new SqlParameter("@FileName", fileName);
            _dbContext.Database.ExecuteSqlRaw(sql, param);
        }

        public void MarkTLAutoDocketActionWorkflowAsGenerated(int actId)
        {
            var sql = "Update tblTmkActionDue Set AutoDocketWorkflowStatus=Case When AutoDocketWorkflowStatus=1 then 2 When AutoDocketWorkflowStatus=3 then 4 else AutoDocketWorkflowStatus  end Where ActId=@ActId";
            var actIdParam = new SqlParameter("@ActId", actId);
            _dbContext.Database.ExecuteSqlRaw(sql, actIdParam);
        }

        public async Task<List<EmailNotificationDTO>> GetTrademarkWatchRecipients(int tlTmkId, int docId) {
            var emailInfo = await _dbContext.EmailNotificationDTO.FromSqlInterpolated($"exec procTmkTrademarkWatch @TLTmkId={tlTmkId},@DocId={docId}").AsNoTracking().ToListAsync();
            return emailInfo;
        }

        public IQueryable<TLSearch> TLSearchRecords => _dbContext.TLSearchRecords;
        public IQueryable<TLSearchDocument> TLSearchDocuments => _dbContext.TLSearchDocuments;
        public IQueryable<TLSearchImage> TLSearchImages => _dbContext.TLSearchImages;
    }
}


