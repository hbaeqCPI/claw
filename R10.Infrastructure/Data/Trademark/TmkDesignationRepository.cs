using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace R10.Infrastructure.Data.Trademark
{
    public class TmkDesignationRepository : ITmkDesignationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public TmkDesignationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int tmkId)
        {
            var list = await _dbContext.LookupDescDTO.FromSqlInterpolated($@"Select DesCountry as Value, DesCaseType + '|' + cast(isnull(SingleClassApplication,0) as varchar) as Text,CountryName as Description From
                                (Select Distinct dc.DesCountry, dc.DesCaseType,c.CountryName,c.SingleClassApplication,
                                row_number() over(partition by dc.DesCountry order by dc.DefaultCaseType desc) as rowno
                                From tblTmkDesCaseType AS dc
                                Inner Join tblTmkCountry c on c.Country=dc.DesCountry
                                Where((dc.[IntlCode] = {country}) AND(dc.[CaseType] = {caseType}))
                                And Not Exists(Select 1 From tblTmkDesignatedCountry AS d Where(d.TmkId = {tmkId}) AND(d.DesCountry = dc.DesCountry))
                                ) t Where rowno = 1").AsNoTracking().ToListAsync();
            return list.Select(item => new { DesCountry = item.Value, DesCaseType = item.Text.Split('|')[0], CountryName = item.Description, GenSingleClassApp = item.Text.Split('|')[1] }).ToArray();
        }

        public async Task<object[]> GetSelectableDesignatedCountriesMultiple(string country, string caseType)
        {
            var list = await _dbContext.LookupDescDTO.FromSqlInterpolated($@"Select DesCountry as Value, DesCaseType + '|' + cast(isnull(SingleClassApplication,0) as varchar) as Text,CountryName as Description From
                                (Select Distinct dc.DesCountry, dc.DesCaseType,c.CountryName,c.SingleClassApplication,
                                row_number() over(partition by dc.DesCountry order by dc.DefaultCaseType desc) as rowno
                                From tblTmkDesCaseType AS dc
                                Inner Join tblTmkCountry c on c.Country=dc.DesCountry
                                Where((dc.[IntlCode] = {country}) AND(dc.[CaseType] = {caseType}))) t Where rowno = 1").AsNoTracking().ToListAsync();
            return list.Select(item => new { DesCountry = item.Value, DesCaseType = item.Text.Split('|')[0], CountryName = item.Description,GenSingleClassApp= item.Text.Split('|')[1] }).ToArray();
        }

        public async Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry)
        {
            return await _dbContext.TmkDesCaseTypes.Where(dc => dc.IntlCode == country && dc.CaseType == caseType && dc.DesCountry == desCountry)
                .Select(dc => dc.DesCaseType).Distinct().ToArrayAsync();
        }

        public async Task DesignateCountries(int tmkId, bool fromCountryLaw, string createdBy)
        {
            var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("TmkId", SqlDbType.Int) });
            record.SetInt32(0, tmkId);
            var tmkIds = new List<SqlDataRecord> { record };

            using (SqlCommand cmd = new SqlCommand("procTmkDesCtry_Gen"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@TmkIds");
                cmd.Parameters.AddWithValue("@TmkIds", tmkIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters["@DesigFromCountryLaw"].Value = fromCountryLaw;
                cmd.Parameters["@CreatedBy"].Value = createdBy;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<TmkDesignatedCountry>> GetSelectableCountries(int tmkId)
        {
            //await _dbContext.Database.ExecuteSqlRawAsync($"Update d Set d.GenApp=0 From tblTmkDesignatedCountry d Inner Join tblTmkTrademark tmk on d.GenCaseNumber=tmk.CaseNumber and d.DesCountry=tmk.Country and d.GenSubCase=tmk.Subcase Where d.TmkId ={tmkId}");
            return await _dbContext.TmkDesignatedCountries.Where(d => d.TmkId == tmkId && d.GenApp)
                .AsNoTracking().Select(d => new TmkDesignatedCountry { DesCountry = d.DesCountry, CountryName = d.Country.CountryName, GenSubCase=d.GenSubCase, GenTmkId = d.GenTmkId }).ToListAsync();
        }

        public async Task GenerateTrademarks(int parentTmkId, string desCountries, string updatedBy)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"procTmkDesCtry_Gen_Apps @TmkId={parentTmkId}, @DesCountries={desCountries},@UpdatedBy={updatedBy}");
        }

        public async Task<TmkDesignatedCountry> GetDesignatedCountry(int desId)
        {
            return await _dbContext.TmkDesignatedCountries.Where(d => d.DesId == desId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<TmkDesignatedCountry>> GetDesignatedCountries(int tmkId)
        {
            return await _dbContext.TmkDesignatedCountries.Where(d => d.TmkId == tmkId).OrderBy(d=> d.Country).ThenBy(d=> d.GenSubCase).AsNoTracking().ToListAsync();
        }

        public async Task DesignatedCountriesUpdate(int tmkId, string userName, IEnumerable<TmkDesignatedCountry> updatedDesignatedCountries, 
                                IEnumerable<TmkDesignatedCountry> newDesignatedCountries, IEnumerable<TmkDesignatedCountry> deletedDesignatedCountries)
        {

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == tmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;

                foreach (var item in deletedDesignatedCountries)
                {
                    _dbContext.Set<TmkDesignatedCountry>().Remove(item);
                }

                foreach (var item in updatedDesignatedCountries)
                {
                    item.GenSubCase = item.GenSubCase ?? "";
                    _dbContext.Entry(item).State = EntityState.Modified;
                }

                foreach (var item in newDesignatedCountries)
                {
                    item.GenCaseNumber = trademark.CaseNumber;
                    item.GenSubCase = item.GenSubCase ?? "";
                    _dbContext.Add(item);
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DesignatedCountriesDelete(TmkDesignatedCountry deletedDesignatedCountry)
        {
            _dbContext.Set<TmkDesignatedCountry>().Remove(deletedDesignatedCountry);
            var trademark = await _dbContext.TmkTrademarks.FirstOrDefaultAsync(t => t.TmkId == deletedDesignatedCountry.TmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = deletedDesignatedCountry.UpdatedBy;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
