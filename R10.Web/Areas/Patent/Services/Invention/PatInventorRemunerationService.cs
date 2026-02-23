using AutoMapper;
using iText.IO.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Services;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Models;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace R10.Web.Areas.Patent.Services
{
    public class PatInventorRemunerationService : IPatInventorRemunerationService
    {
        private readonly IChildEntityService<PatIRRemuneration, PatInventorInv> _patInventorInvService;
        private readonly IChildEntityService<PatIRRemuneration, PatIRProductSale> _patProductSaleService;
        private readonly IEntityService<PatIRDistribution> _patDistributionEntityService;
        private readonly IPatInventorService _patInventorService;
        private readonly IEntityService<PatIREmployeePosition> _patEmployeePositionService;
        private readonly IEntityService<PatIRValorizationRule> _patValorizationRuleEntityService;
        private readonly IEntityService<PatIRRemuneration> _patRemunerationEntityService;
        private readonly IPatInventorFRRemunerationService _patInventorFRRemunerationService;
        private readonly IEntityService<PatIRStaggering> _patStaggeringEntityService;
        private readonly IChildEntityService<PatIRStaggering, PatIRStaggeringDetail> _patStaggeringDetailService;
        private readonly IEntityService<PatIREuroExchangeRate> _patEuroExchangeRateService;
        private readonly IChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly> _patEuroExchangeRateYearlyService;
        private readonly ICountryApplicationService _applicationService;
        private readonly IProductService _productService;
        private readonly IInventionService _inventionService;
        private readonly IEntityService<PatIRRemunerationFormula> _iRRemunerationFormulaService;
        private readonly IEntityService<PatIRRemunerationFormulaFactor> _iRFormulaFactorService;
        private readonly IEntityService<PatIRRemunerationValuationMatrixType> _iRValuationMatrixTypeService;
        private readonly IEntityService<PatIRRemunerationValuationMatrix> _iRValuationMatrixService;
        private readonly IChildEntityService<PatIRRemunerationValuationMatrix, PatIRRemunerationValuationMatrixCriteria> _iRValuationMatrixCriteriaService;
        private readonly IChildEntityService<PatIRRemuneration, PatIRRemunerationValuationMatrixData> _iRValuationMatrixDataService;
        private readonly IProductSaleService _saleService;
        private readonly IChildEntityService<Invention, PatProductInv> _patProductInvService;
        private readonly IEntityService<PatIREmployeePosition> _patInventorPositionService;

        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly IStringLocalizer<InventionResource> _localizer;

        public PatInventorRemunerationService(
            IChildEntityService<PatIRRemuneration, PatInventorInv> patInventorInvService,
            IChildEntityService<PatIRRemuneration, PatIRProductSale> patProductSaleService,
            IEntityService<PatIRDistribution> patDistributionEntityService,
            IPatInventorService patInventorService,
            IEntityService<PatIREmployeePosition> patEmployeePositionService,
            IEntityService<PatIRValorizationRule> patValorizationRuleEntityService,
            IEntityService<PatIRRemuneration> patRemunerationEntityService,
            IEntityService<PatIRStaggering> patStaggeringEntityService,
            IPatInventorFRRemunerationService patInventorFRRemunerationService,
            IChildEntityService<PatIRStaggering, PatIRStaggeringDetail> patStaggeringDetailService,
            IEntityService<PatIREuroExchangeRate> patEuroExchangeRateService,
            IChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly> patEuroExchangeRateYearlyService,
            ICountryApplicationService applicationService,
            IProductService productService,
            IInventionService inventionService,
            IEntityService<PatIRRemunerationFormula> iRRemunerationFormulaService,
            IEntityService<PatIRRemunerationFormulaFactor> iRFormulaFactorService,
            IEntityService<PatIRRemunerationValuationMatrixType> iRValuationMatrixTypeService,
            IEntityService<PatIRRemunerationValuationMatrix> iRValuationMatrixService,
            IChildEntityService<PatIRRemunerationValuationMatrix, PatIRRemunerationValuationMatrixCriteria> iRValuationMatrixCriteriaService,
            IChildEntityService<PatIRRemuneration, PatIRRemunerationValuationMatrixData> iRValuationMatrixDataService,
            IProductSaleService saleService,
            IChildEntityService<Invention, PatProductInv> patProductInvService,
            IEntityService<PatIREmployeePosition> patInventorPositionService,
            IMapper mapper,
            ISystemSettings<PatSetting> patSettings,
            IStringLocalizer<InventionResource> localizer
            )
        {
            _patInventorInvService = patInventorInvService;
            _patProductSaleService = patProductSaleService;
            _patDistributionEntityService = patDistributionEntityService;
            _patInventorService = patInventorService;
            _patEmployeePositionService = patEmployeePositionService;
            _patValorizationRuleEntityService = patValorizationRuleEntityService;
            _patRemunerationEntityService = patRemunerationEntityService;
            _patInventorFRRemunerationService = patInventorFRRemunerationService;
            _patStaggeringEntityService = patStaggeringEntityService;
            _patStaggeringDetailService = patStaggeringDetailService;
            _patEuroExchangeRateService = patEuroExchangeRateService;
            _patEuroExchangeRateYearlyService = patEuroExchangeRateYearlyService;
            _applicationService = applicationService;
            _inventionService = inventionService;
            _productService = productService;
            _iRRemunerationFormulaService = iRRemunerationFormulaService;
            _iRFormulaFactorService = iRFormulaFactorService;
            _iRValuationMatrixTypeService = iRValuationMatrixTypeService;
            _iRValuationMatrixService = iRValuationMatrixService;
            _iRValuationMatrixCriteriaService = iRValuationMatrixCriteriaService;
            _iRValuationMatrixDataService = iRValuationMatrixDataService;
            _saleService = saleService;
            _patProductInvService = patProductInvService;
            _patInventorPositionService = patInventorPositionService;
            _mapper = mapper;
            _patSettings = patSettings;
            _localizer = localizer;
        }

        public async Task<PatIRRemuneration> GetByInvIdAsync(int invId)
        {
            return await _patRemunerationEntityService.QueryableList.AsNoTracking().SingleOrDefaultAsync(i => i.InvId == invId);
        }

        public async Task InventorListSave(int remunerationId, List<PatInventorInv> updated, string userName)
        {
            if (updated.Any())
            {
                var inventors = _patInventorService.QueryableList;
                foreach (var u in updated)
                {
                    if (u.PositionId == null)
                    {
                        var inventor = inventors.FirstOrDefault(c => c.InventorID == u.InventorID);
                        if (inventor != null)
                        {
                            u.PositionId = inventor.PositionId;
                            var inventorPosition = _patInventorPositionService.QueryableList.FirstOrDefault(c => c.PositionId == inventor.PositionId);
                            u.PositionA = inventorPosition != null && u.PositionA == null ? inventorPosition.PositionA : u.PositionA;
                            u.PositionB = inventorPosition != null && u.PositionB == null ? inventorPosition.PositionB : u.PositionB;
                            u.PositionC = inventorPosition != null && u.PositionC == null ? inventorPosition.PositionC : u.PositionC;
                        }
                    }

                    if (!u.PaidByLumpSum)
                    {
                        u.LumpSumAmount = null;
                        u.LumpSumPaidDate = null;
                    }
                    else
                    {
                        if (u.LumpSumPaidDate == null)
                        {
                            u.LumpSumAmount = await GetLumpSumAmount(remunerationId, _mapper.Map<InventionInventorRemunerationInventorInfoViewModel>(u));
                        }
                    }
                }

                await _patInventorInvService.Update(remunerationId, userName,
                    updated,
                    new List<PatInventorInv>(),
                    new List<PatInventorInv>()
                    );

                await UpdateDistribution(remunerationId, updated);

            }
        }

        public async Task UpdateDistribution(int remunerationId, List<PatIRProductSale> added, List<PatIRProductSale> updated, List<PatIRProductSale> deleted)
        {
            var settings = _patSettings.GetSetting().Result;
            if (!(settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly"))
                return;

            var earliestUpdatedYear = new[] { added.Min(c => c.Year) ?? int.MaxValue, updated.Min(c => c.Year) ?? int.MaxValue, deleted.Min(c => c.Year) ?? int.MaxValue }.Min();
            await UpdateDistributionAfterYear(remunerationId, earliestUpdatedYear);
        }

        public async Task UpdateDistributionAfterYear(int remunerationId, int currentYear)
        {
            var settings = _patSettings.GetSetting().Result;
            if (!(settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly"))
                return;

            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.RemunerationId == remunerationId && !c.PaidByLumpSum).ToList(); //&& c.InventorInvInventor.Citizenship != null && c.InventorInvInventor.Citizenship.ToUpper().Equals("DE") && !c.PaidByLumpSum);
            var PatIRProductSales = _patProductSaleService.QueryableList.Where(c => c.RemunerationId == remunerationId).ToList();
            var PatIRDistributions = _patDistributionEntityService.QueryableList.ToList();

            List<int> updatedYears = PatIRProductSales.Where(c => c.Year >= currentYear).Select(c => (int)c.Year).Distinct().ToList();
            updatedYears.AddRange(PatIRDistributions.Where(d => d.Year >= currentYear).Select(c => (int)c.Year).Distinct().ToList());
            if (!updatedYears.Contains(currentYear))
                updatedYears.Add(currentYear);

            List<PatIRDistribution> addedDistributions = new List<PatIRDistribution>();
            List<PatIRDistribution> updatedDistributions = new List<PatIRDistribution>();
            List<PatIRDistribution> deletedDistributions = new List<PatIRDistribution>();

            foreach (int year in updatedYears.Distinct())
            {
                foreach (PatInventorInv invt in PatInventorInvs)
                {
                    if (invt.PaidByLumpSum)
                    {
                        deletedDistributions.AddRange(PatIRDistributions.Where(c => c.InventorInvID == invt.InventorInvID && c.Year == year && c.PaidDate == null));
                    }
                    else
                    {
                        var distribution = PatIRDistributions.FirstOrDefault(c => c.InventorInvID == invt.InventorInvID && c.Year == year);
                        if (distribution == null)
                        {
                            if (PatIRProductSales.Any(c => c.Year == year))
                            {
                                PatIRDistribution add = new PatIRDistribution
                                {
                                    InventorInvID = invt.InventorInvID,
                                    Year = year,
                                    Amount = await CalculateDistributionValue(year, PatIRProductSales, invt)
                                };
                                addedDistributions.Add(add);
                            }
                        }
                        else if (distribution.PaidDate == null)
                        {
                            if (PatIRProductSales.Any(c => c.Year == year))
                            {
                                if (!distribution.UseOverrideAmount)
                                    distribution.Amount = await CalculateDistributionValue(year, PatIRProductSales, invt);
                                updatedDistributions.Add(distribution);
                            }
                            else
                            {
                                deletedDistributions.Add(distribution);
                            }
                        }
                    }

                }
            }

            await _patDistributionEntityService.Add(addedDistributions);
            await _patDistributionEntityService.Update(updatedDistributions);
            await _patDistributionEntityService.Delete(deletedDistributions);
        }

        private async Task<double> CalculateDistributionValue(int year, List<PatIRProductSale> productSales, PatInventorInv invt)
        {
            double result = 0;
            var patSettings = await _patSettings.GetSetting();
            var PercentageOfOwnerShip = await GetTotalInventorFactor(invt);
            var PercentageOfInvention = (invt.Percentage ?? 0) / 100;

            var sales = productSales.Where(c => c.Year == year);

            var formulas = _iRRemunerationFormulaService.QueryableList.Where(c => c.RemunerationType.Equals("Yearly")).OrderByDescending(c => c.EffStartDate);
            var calculateDateString = patSettings.InventorRemunerationCalculateDate + "/" + year;
            var calculateDate = DateTime.Parse(calculateDateString);
            var formula = formulas.FirstOrDefault(c => (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate != null && c.EffEndDate >= calculateDate));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => ((c.EffStartDate == null) && (c.EffEndDate != null && c.EffEndDate >= calculateDate)) || (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate == null));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => c.EffStartDate == null && c.EffEndDate == null);

            var formulaText = "";
            if (formula != null)
                formulaText = formula.Formula;
            else
                formulaText = patSettings.InventorRemunerationDefaultFormula;

            formulaText = formulaText.Replace("{%ofOwnership}", PercentageOfOwnerShip.ToString());
            formulaText = formulaText.Replace("{%ofInvention}", PercentageOfInvention.ToString());
            formulaText = formulaText.Replace("{InitialPayment}", (invt.InitialPayment ?? 0).ToString());

            var formulaFactors = _iRFormulaFactorService.QueryableList.ToList();
            var formulaFactorData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FactorId != null && c.RemunerationId == invt.RemunerationId).ToList();

            foreach (var factor in formulaFactors)
            {
                var currenctfactorData = formulaFactorData.FirstOrDefault(c => c.FactorId == factor.FactorId);
                double value = 0;
                if (currenctfactorData != null)
                    value = currenctfactorData.ActualValue ?? 1;
                else
                    value = factor.DefaultValue ?? 1;
                formulaText = formulaText.Replace("{" + (string)factor.Variable + "}", value.ToString());
            }

            if (formulaText.Contains("{LicenseFactor}") || formulaText.Contains("{Revenue}") || formulaText.Contains("{InventionValue}") || formulaText.Contains("{RevenueForRemuneration}"))
            {
                RevenueForRemunerationViewModel revenueForRemuneration = new RevenueForRemunerationViewModel();
                if (formulaText.Contains("{RevenueForRemuneration}"))
                {
                    revenueForRemuneration = await GetRevenueForRemuneration(year, productSales, (int)invt.RemunerationId);
                }

                foreach (var sale in sales)
                {
                    double rate = 0;
                    if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                    {
                        rate = 1;
                    }
                    else
                    {
                        var er = _patEuroExchangeRateService.QueryableList.FirstOrDefault(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                        if (er == null)
                            throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);
                        if (er.UseDefault)
                        {
                            rate = er.DefaultExchangeRate;
                        }
                        else
                        {
                            var ery = _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefault(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                            if (ery == null)
                                throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                            rate = ery.ExchangeRate;
                        }
                    }

                    var formulaTextCopy = String.Copy(formulaText);
                    formulaTextCopy = formulaTextCopy.Replace("{LicenseFactor}", ((sale.LicenseFactor ?? 0) / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{InventionValue}", ((sale.InventionValue ?? 0) / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{Revenue}", ((sale.Revenue ?? 0) / rate).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{RevenueForRemuneration}", (revenueForRemuneration.RevenueForRemuneration * ((sale.Revenue ?? 0) / rate) / (revenueForRemuneration.TotalRevenueThisYear == 0 ? 1 : revenueForRemuneration.TotalRevenueThisYear)).ToString());
                    formulaTextCopy = ReplaceVariableToOne(formulaTextCopy);
                    DataTable dt = new DataTable();
                    var ComputeResult = dt.Compute(formulaTextCopy, "");
                    result += double.Parse(ComputeResult.ToString());
                }
            }
            else
            {
                formulaText = ReplaceVariableToOne(formulaText);
                DataTable dt = new DataTable();
                var ComputeResult = dt.Compute(formulaText, "");
                result = double.Parse(ComputeResult.ToString());
            }

            if (formula != null)
            {
                if (formula.MaxValue != null && result > formula.MaxValue)
                    result = formula.MaxValue ?? 1;
                if (formula.MinValue != null && result < formula.MinValue)
                    result = formula.MinValue ?? 1;
            }

            return result.RoundTo2ndDecimals();
        }

        public async Task<double> CalculateDistributionValue(PatIRDistribution distribution)
        {
            int year = (int)distribution.Year;
            var invt = _patInventorInvService.QueryableList.First(c => c.InventorInvID == distribution.InventorInvID);
            var PatIRProductSales = _patProductSaleService.QueryableList.Where(c => c.RemunerationId == invt.RemunerationId && c.Year <= year).ToList();
            return await CalculateDistributionValue(year, PatIRProductSales, invt);
        }

        public async Task<RevenueForRemunerationViewModel> GetRevenueForRemuneration(int year, List<PatIRProductSale> productSales, int remunerationId)
        {
            RevenueForRemunerationViewModel rfr = new RevenueForRemunerationViewModel() { RevenueForRemuneration = 0, TotalRevenueThisYear = 0 };
            double TotalRevenuePreviousYear = 0;
            var salesThisYear = productSales.Where(c => c.Year == year).ToList();
            var salesPreviousYear = productSales.Where(c => c.Year < year).ToList();

            foreach (var sale in salesThisYear)
            {
                double rate = 0;
                if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                {
                    rate = 1;
                }
                else
                {
                    var er = await _patEuroExchangeRateService.QueryableList.FirstOrDefaultAsync(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                    if (er == null)
                        throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);

                    if (er.UseDefault)
                    {
                        rate = er.DefaultExchangeRate;
                    }
                    else
                    {
                        var ery = await _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefaultAsync(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                        if (ery == null)
                            throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                        rate = ery.ExchangeRate;
                    }
                }

                rfr.TotalRevenueThisYear += (sale.Revenue ?? 0) / rate;
            }

            foreach (var sale in salesPreviousYear)
            {
                double rate = 0;
                if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                {
                    rate = 1;
                }
                else
                {
                    var er = await _patEuroExchangeRateService.QueryableList.FirstOrDefaultAsync(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                    if (er == null)
                        throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);
                    if (er.UseDefault)
                    {
                        rate = er.DefaultExchangeRate;
                    }
                    else
                    {
                        var ery = await _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefaultAsync(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                        if (ery == null)
                            throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                        rate = ery.ExchangeRate;
                    }
                }

                TotalRevenuePreviousYear += (sale.Revenue ?? 0) / rate;
            }

            var staggering = await _patStaggeringEntityService.QueryableList.FirstOrDefaultAsync(s => s.IsActive == true);
            if (staggering == null)
                throw new NullReferenceException(_localizer["Staggering table is not found."]);

            var staggeringDetails = await _patStaggeringDetailService.QueryableList.Where(c => c.StaggeringId == staggering.StaggeringId).OrderBy(c => c.AmountFrom).ToListAsync();
            if (staggeringDetails == null)
                throw new NullReferenceException(_localizer["Staggering table is not found."]);

            if (staggeringDetails.Count() == 0)
            {
                rfr.RevenueForRemuneration = rfr.TotalRevenueThisYear;
            }
            else
            {
                var totalRevenuePreviousYear = TotalRevenuePreviousYear;
                var totalRevenueToThisYear = rfr.TotalRevenueThisYear + totalRevenuePreviousYear;
                staggeringDetails = staggeringDetails.Where(c => (c.AmountTo == null || c.AmountTo >= totalRevenuePreviousYear) &&
                                                                 (c.AmountFrom == null || c.AmountFrom < totalRevenueToThisYear)).OrderBy(c => c.AmountFrom).OrderBy(c => c.AmountFrom).ToList();
                double revenueForRemuneration = 0;

                foreach (var detail in staggeringDetails)
                {
                    /// F from T to P previous W revenue to this year
                    // F P
                    if (detail.AmountFrom == null || totalRevenuePreviousYear > detail.AmountFrom)
                    {
                        // F P W T
                        if (detail.AmountTo == null || totalRevenueToThisYear <= detail.AmountTo)
                        {
                            revenueForRemuneration += (totalRevenueToThisYear - totalRevenuePreviousYear) * (1 - detail.Reduction);
                        }
                        else //F P T W
                        {
                            revenueForRemuneration += ((detail.AmountTo ?? 0) - totalRevenuePreviousYear) * (1 - detail.Reduction);
                        }
                    }
                    else // P F
                    {
                        // P F W T
                        if (detail.AmountTo == null || totalRevenueToThisYear <= detail.AmountTo)
                        {
                            revenueForRemuneration += (totalRevenueToThisYear - (detail.AmountFrom ?? 0)) * (1 - detail.Reduction);
                        }
                        else //P F T W
                        {
                            revenueForRemuneration += ((detail.AmountTo ?? 0) - (detail.AmountFrom ?? 0)) * (1 - detail.Reduction);
                        }
                    }
                }

                rfr.RevenueForRemuneration = revenueForRemuneration;
            }

            return rfr;
        }


        public async Task DeleteDistribution(PatIRProductSale deleted)
        {
            var settings = _patSettings.GetSetting().Result;
            if (!(settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly"))
                return;

            await UpdateDistributionAfterYear(deleted.RemunerationId, (int)deleted.Year);
        }

        public async Task UpdateDistribution(int remunerationId, List<PatInventorInv> updated)
        {
            var settings = _patSettings.GetSetting().Result;
            if (!(settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly"))
                return;

            var PatIRProductSales = _patProductSaleService.QueryableList.Where(c => c.RemunerationId == remunerationId).ToList();
            var PatIRDistributions = _patDistributionEntityService.QueryableList.ToList();

            List<int> updatedYears = new List<int>();
            foreach (var patIRProductSale in PatIRProductSales)
            {
                updatedYears.Add((int)patIRProductSale.Year);
            }
            updatedYears = updatedYears.Distinct().ToList();

            List<PatIRDistribution> addedDistributions = new List<PatIRDistribution>();
            List<PatIRDistribution> deletedDistributions = new List<PatIRDistribution>();
            List<PatIRDistribution> updatedDistributions = new List<PatIRDistribution>();

            foreach (int year in updatedYears)
            {
                foreach (PatInventorInv invt in updated)
                {
                    if (invt.PaidByLumpSum)
                    {
                        deletedDistributions.AddRange(PatIRDistributions.Where(c => c.InventorInvID == invt.InventorInvID && c.Year == year && c.PaidDate == null));
                    }
                    else
                    {
                        var distribution = PatIRDistributions.FirstOrDefault(c => c.InventorInvID == invt.InventorInvID && c.Year == year);
                        if (distribution == null)
                        {
                            PatIRDistribution add = new PatIRDistribution
                            {
                                InventorInvID = invt.InventorInvID,
                                Year = year,
                                Amount = await CalculateDistributionValue(year, PatIRProductSales, invt)
                            };
                            addedDistributions.Add(add);
                        }
                        else if (distribution.PaidDate == null)
                        {
                            if (!distribution.UseOverrideAmount)
                                distribution.Amount = await CalculateDistributionValue(year, PatIRProductSales, invt);
                            updatedDistributions.Add(distribution);
                        }
                    }
                }
            }

            await _patDistributionEntityService.Add(addedDistributions);
            await _patDistributionEntityService.Update(updatedDistributions);
            await _patDistributionEntityService.Delete(deletedDistributions);
        }

        private async Task<double> GetTotalInventorFactor(PatInventorInv invt)
        {
            return (double)(await CalculateInventorPosition(((invt.PositionA ?? 0) + (invt.PositionB ?? 0) + (invt.PositionC ?? 0)))) / 100;
        }

        public async Task<InventionInventorRemunerationTotalCostViewModel> GetInventorRemunerationTotalCost(int invId)
        {
            var invention = await _inventionService.QueryableList.FirstOrDefaultAsync(i => i.InvId == invId);
            if (invention == null)
                return new InventionInventorRemunerationTotalCostViewModel();

            var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(r => r.InvId == invId);

            //return French Remuneration Cost if French Remuneration on
            if (invention.UseInventorFRRemuneration && !invention.UseInventorRemuneration)
            {
                return _patInventorFRRemunerationService.GetInventorRemunerationTotalCost(invention);
            }

            InventionInventorRemunerationTotalCostViewModel viewModel = new InventionInventorRemunerationTotalCostViewModel()
            {
                InvId = invention.InvId,
                CaseNumber = invention.CaseNumber,
                CompensationEndDate = remuneration != null ? remuneration.CompensationEndDate : null,
                TotalCost = await GetTotalCost(invention),
                YearlyCost = await GetYearlyCost(invention)
            };

            return viewModel;
        }

        public async Task<double> GetTotalCost(Invention invention)
        {
            double totalCost = 0;
            var inventors = await _patInventorInvService.QueryableList.Where(i => i.InvId == invention.InvId).ToListAsync();

            foreach (var inventor in inventors)
            {
                var distribution = await _patDistributionEntityService.QueryableList.Where(c => c.InventorInvID == inventor.InventorInvID).ToListAsync();
                totalCost += distribution.Sum(c => (c.Amount));
            }

            var inventorInvs = await _patInventorInvService.QueryableList.Where(c => c.InvId == invention.InvId && c.LumpSumAmount != null && c.LumpSumPaidDate != null).ToListAsync();

            foreach (var inventorInv in inventorInvs)
            {
                totalCost += inventorInv.LumpSumAmount ?? 0;
            }

            return totalCost;
        }

        public async Task<List<InventionInventorRemunerationYearlyCostViewModel>> GetYearlyCost(Invention invention)
        {
            var YearlyCost = new List<InventionInventorRemunerationYearlyCostViewModel>();
            var inventors = await _patInventorInvService.QueryableList.Where(i => i.InvId == invention.InvId).ToListAsync();

            var distributions = await _patDistributionEntityService.QueryableList.Where(c => inventors.Select(i => i.InventorInvID).ToList().Contains(c.InventorInvID)).ToListAsync();
            var years = distributions.Select(c => new { Year = c.Year }).OrderBy(c => c.Year).Distinct();

            foreach (var year in years)
            {
                var distribution = distributions.Where(c => c.Year == year.Year);
                var viewModel = new InventionInventorRemunerationYearlyCostViewModel()
                {
                    Year = (int)year.Year,
                    Cost = distribution.Sum(c => c.Amount)
                };

                YearlyCost.Add(viewModel);
            }

            var inventorInvs = await _patInventorInvService.QueryableList.Where(c => c.InvId == invention.InvId && c.LumpSumAmount != null && c.LumpSumPaidDate != null).ToListAsync();

            foreach (var inventorInv in inventorInvs)
            {
                int year = ((DateTime)inventorInv.LumpSumPaidDate).Year;
                if (YearlyCost.Where(c => c.Year == year).Count() > 0)
                {
                    YearlyCost.FirstOrDefault(c => c.Year == year).Cost += inventorInv.LumpSumAmount ?? 0;
                }
                else
                {
                    var viewModel = new InventionInventorRemunerationYearlyCostViewModel()
                    {
                        Year = year,
                        Cost = inventorInv.LumpSumAmount ?? 0
                    };

                    YearlyCost.Add(viewModel);
                }
            }

            return YearlyCost;
        }

        public async Task<int> CalculateInventorPosition(int? sum)
        {
            if (sum == null)
                return 0;

            var valorizationRule = await _patValorizationRuleEntityService.QueryableList.FirstOrDefaultAsync(c => (int)c.Point == (int)sum);
            if (valorizationRule == null)
                return 0;

            return valorizationRule.Ratio ?? 0;
        }

        public async Task InitRemuneration(Invention invention)
        {
            if (invention.InvId > 0)
            {
                if (!_patRemunerationEntityService.QueryableList.Any(c => c.InvId == invention.InvId))
                {
                    //Create Remuneration record
                    PatIRRemuneration remuneration = new PatIRRemuneration()
                    {
                        InvId = invention.InvId,
                        CreatedBy = invention.UpdatedBy,
                        DateCreated = DateTime.Now,
                        UpdatedBy = invention.UpdatedBy,
                        LastUpdate = DateTime.Now
                    };
                    await _patRemunerationEntityService.Add(remuneration);

                    var settings = _patSettings.GetSetting().Result;
                    var noOfInvetorsToAward = settings.InventorRemunerationNoInventors;
                    if (noOfInvetorsToAward == 0)
                        noOfInvetorsToAward = int.MaxValue;
                    //Update Inventors to equal share
                    var PatInventorInvs = await GetAvailablePatInventorInvs(invention.InvId);

                    var inventors = _patInventorService.QueryableList;
                    var inventorPositions = _patEmployeePositionService.QueryableList;
                    PatInventorInvs.ForEach(c =>
                    {
                        c.Percentage = c.Percentage ?? 0;
                        c.RemunerationId = remuneration.RemunerationId;
                        c.InventorInvInvention = null;
                        var inventor = inventors.FirstOrDefault(d => d.InventorID == c.InventorID);
                        c.PositionId = inventor == null ? null : inventor.PositionId;
                        var inventionPosition = inventorPositions.FirstOrDefault(d => d.PositionId == c.PositionId);
                        c.PositionA = inventionPosition == null ? null : inventionPosition.PositionA;
                        c.PositionB = inventionPosition == null ? null : inventionPosition.PositionB;
                        c.PositionC = inventionPosition == null ? null : inventionPosition.PositionC;
                    });

                    if (!PatInventorInvs.Any(d => d.Percentage > 0))
                    {
                        var count = PatInventorInvs.Count() > noOfInvetorsToAward ? noOfInvetorsToAward : PatInventorInvs.Count();
                        if (count == 0)
                            count = 1;

                        double equalSharePercentage = (double)(100.0 / count).RoundTo2ndDecimals();

                        if (PatInventorInvs.Count() != 0)
                            PatInventorInvs.OrderBy(c => c.OrderOfEntry).Take(count).ToList().ForEach(c =>
                            {
                                c.Percentage = equalSharePercentage;
                            });

                    }

                    await _patInventorInvService.Update(remuneration.RemunerationId, invention.UpdatedBy, PatInventorInvs, new List<PatInventorInv>() { }, new List<PatInventorInv>() { });

                    //Create Init Product Sale
                    if ((settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly") && settings.IsInventorRemunerationUsingProductSalesOn)
                    {
                        List<PatIRProductSale> productSales = new List<PatIRProductSale>();
                        var productInvs = await _patProductInvService.QueryableList.Where(p => p.InvId == invention.InvId).Include(p => p.Product).ToListAsync();
                        foreach (var productInv in productInvs)
                        {
                            var sales = await _saleService.QueryableList.Where(s => s.ProductId == productInv.ProductId).ToListAsync();
                            foreach (var sale in sales.Where(s => !string.IsNullOrEmpty(s.CurrencyType)))
                            {
                                if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > sale.Yr)
                                    continue;

                                if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < sale.Yr)
                                    continue;

                                var irProductSale = new PatIRProductSale
                                {
                                    RemunerationId = remuneration.RemunerationId,
                                    Product = productInv.Product.ProductName,
                                    Year = sale.Yr,
                                    Country = sale.Country,
                                    LicenseFactor = productInv.LicenseFactor ?? 0,
                                    InventionValue = productInv.InventionValue ?? 0,

                                    Revenue = (double)sale.Net,
                                    UseOverrideRevenue = false,
                                    CurrencyType = sale.CurrencyType,
                                    DateCreated = DateTime.Now,
                                    LastUpdate = DateTime.Now,
                                    CreatedBy = invention.UpdatedBy,
                                    UpdatedBy = invention.UpdatedBy,
                                };

                                productSales.Add(irProductSale);
                            }
                        }

                        if (productSales.Any())
                            await _patProductSaleService.Add(productSales);

                        await UpdateDistribution(remuneration.RemunerationId, productSales, new List<PatIRProductSale>(), new List<PatIRProductSale>());

                    }
                }
            }
        }

        public async Task UpdateCompensationEndDate(int invId, DateTime? compensationEndDate, string userName)
        {
            var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invId);
            if (remuneration == null)
                return;

            remuneration.CompensationEndDate = compensationEndDate;
            remuneration.UpdatedBy = userName;
            remuneration.LastUpdate = DateTime.Now;
            await _patRemunerationEntityService.Update(remuneration);
        }

        public async Task DeleteRemuneration(int invId, bool fromInvention = false)
        {

            var remuneration = _patRemunerationEntityService.QueryableList.FirstOrDefault(c => c.InvId == invId);
            if (remuneration == null)
                return;

            //delete matrix data
            await _iRValuationMatrixDataService.Delete(_iRValuationMatrixDataService.QueryableList.Where(c => c.RemunerationId == remuneration.RemunerationId));

            //delete product sales
            await _patProductSaleService.Delete(_patProductSaleService.QueryableList.Where(c => c.RemunerationId == remuneration.RemunerationId));

            //delete inventor distribution and update inventorInv
            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.InvId == invId).ToList();

            PatInventorInvs.ForEach(c =>
            {
                c.PaidByLumpSum = false;
                c.LumpSumAmount = null;
                c.LumpSumPaidDate = null;
                c.RemunerationId = null;
                c.RemunerationRemarks = null;
                c.InitialPayment = null;
                c.PositionA = null;
                c.PositionB = null;
                c.PositionC = null;
                c.BuyingRightsAmount = null;
                c.BuyingRightsDate = null;
            });
            await _patInventorInvService.Update(PatInventorInvs);

            var InventorInvIds = PatInventorInvs.Select(c => c.InventorInvID).ToList();
            await _patDistributionEntityService.Delete(_patDistributionEntityService.QueryableList.Where(c => InventorInvIds.Contains(c.InventorInvID)));

            //delete remuneration 
            remuneration = _patRemunerationEntityService.QueryableList.FirstOrDefault(c => c.InvId == invId);
            await _patRemunerationEntityService.Delete(remuneration);

            if (!fromInvention)
            {
                //update invention not use remuneration
                var invention = _inventionService.QueryableList.FirstOrDefault(c => c.InvId == invId);
                invention.UseInventorRemuneration = false;
                await _inventionService.Update(invention);
            }
        }

        public async Task<int> GetRemunerationId(int invId)
        {
            int remunerationId = 0;
            var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invId);
            if (remuneration != null)
            {
                remunerationId = remuneration.RemunerationId;
            }
            return remunerationId;
        }

        public async Task<List<PatInventorInv>> GetAvailablePatInventorInvs(int invId)
        {
            var PatInventorInvs = await _patInventorInvService.QueryableList.Where(c => c.InvId == invId && !c.PaidByLumpSum).ToListAsync(); // && c.InventorInvInventor.Citizenship != null && c.InventorInvInventor.Citizenship.ToUpper().Equals("DE") && !c.PaidByLumpSum);
            return PatInventorInvs;
        }

        public async Task<InventorRemunerationValuationMatrixViewModel> GetInventorRemunerationValuationMatrixViewModels(int remunerationId)
        {
            var viewModel = new InventorRemunerationValuationMatrixViewModel();
            var formulaFactors = _mapper.Map<List<FormulaFactorViewModel>>(_iRFormulaFactorService.QueryableList);
            var matrixes = _mapper.Map<List<ValuationMatrixViewModel>>(_iRValuationMatrixService.QueryableList.Where(c => c.ActiveSwitch));
            var criteria = _mapper.Map<List<ValuationMatrixCriteriaViewModel>>(_iRValuationMatrixCriteriaService.QueryableList.Where(c => c.ActiveSwitch));

            var currentData = await _iRValuationMatrixDataService.QueryableList.Where(c => c.RemunerationId == remunerationId).ToListAsync();
            criteria.ForEach(c =>
            {
                var data = currentData.FirstOrDefault(d => c.CriteriaId == d.CriteriaId);
                if (data != null)
                {
                    c.ActualValue = data.ActualValue;
                    c.UseManualEntry = data.UseManualEntry;
                }
                else
                {
                    c.ActualValue = 0;
                    c.UseManualEntry = false;
                }
            });

            foreach (var c in matrixes)
            {
                c.IRMatrixType = await _iRValuationMatrixTypeService.QueryableList.FirstOrDefaultAsync(d => d.MatrixType == c.MatrixType);
                c.ValuationMatrixCriteria = criteria.Where(d => d.MatrixId == c.MatrixId).ToList();
                var data = currentData.FirstOrDefault(d => c.MatrixId == d.MatrixId);
                if (data != null)
                {
                    c.ActualValue = data.ActualValue;
                    c.UseManualEntry = data.UseManualEntry;
                }
                else
                {
                    c.ActualValue = c.DefaultValue;
                    c.UseManualEntry = false;
                }
            }
            ;

            formulaFactors.ForEach(c =>
            {
                var data = currentData.FirstOrDefault(d => (int?)c.FactorId == d.FactorId);
                if (data != null)
                {
                    c.ActualValue = data.ActualValue;
                    c.UseManualEntry = data.UseManualEntry;
                }
                else
                {
                    c.ActualValue = c.DefaultValue;
                    c.UseManualEntry = false;
                }
            });

            viewModel.FormulaFactors = formulaFactors;
            viewModel.ValuationMatrixes = matrixes;

            return viewModel;
        }

        public async Task UpdateMatrixes(InventionInventorRemunerationViewModel matrixData, string userName)
        {
            var manualEntryOptions = PraseManualEntryOptions(matrixData);
            var initData = PraseMatrixData(matrixData, manualEntryOptions);
            var currentData = _iRValuationMatrixDataService.QueryableList.Where(c => c.RemunerationId == matrixData.RemunerationId).ToList();
            var data = await CalculateMatrixData(initData);
            var added = new List<PatIRRemunerationValuationMatrixData>();
            var updated = new List<PatIRRemunerationValuationMatrixData>();
            var deleted = new List<PatIRRemunerationValuationMatrixData>();
            var now = DateTime.Now;

            foreach (var d in data)
            {
                var existingData = currentData.FirstOrDefault(c => NullOrEqual(c.FactorId, d.FactorId) && NullOrEqual(c.MatrixId, d.MatrixId) && NullOrEqual(c.CriteriaId, d.CriteriaId));
                if (existingData != null)
                {
                    if (existingData.ActualValue != d.ActualValue || existingData.UseManualEntry != d.UseManualEntry)
                    {
                        existingData.ActualValue = d.ActualValue;
                        existingData.UseManualEntry = d.UseManualEntry;
                        existingData.UpdatedBy = userName;
                        existingData.LastUpdate = now;
                        updated.Add(existingData);
                    }
                }
                else
                {
                    d.CreatedBy = userName;
                    d.DateCreated = now;
                    d.UpdatedBy = userName;
                    d.LastUpdate = now;
                    added.Add(d);
                }
            }

            await _iRValuationMatrixDataService.Update(matrixData.RemunerationId, userName, updated, added, deleted);
        }

        private bool NullOrEqual(int? a, int? b)
        {
            if ((a == null && b == null) || a == b)
                return true;
            return false;
        }

        private List<ManualEntryOptions> PraseManualEntryOptions(InventionInventorRemunerationViewModel matrixData)
        {
            var result = new List<ManualEntryOptions>();
            if (!String.IsNullOrEmpty(matrixData.MatrixData))
            {
                var matrixDataItem = matrixData.MatrixData.Split("|");
                foreach (var item in matrixDataItem)
                {
                    var itemList = item.Split('~');
                    if (itemList[0].Equals("FactorManualId"))
                    {
                        ManualEntryOptions data = new ManualEntryOptions()
                        {
                            FactorManualId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2])
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("MatrixManualId"))
                    {
                        ManualEntryOptions data = new ManualEntryOptions()
                        {
                            MatrixManualId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2])
                        };
                        result.Add(data);
                    }
                }
            }

            return result;
        }

        private List<PatIRRemunerationValuationMatrixData> PraseMatrixData(InventionInventorRemunerationViewModel matrixData, List<ManualEntryOptions> options)
        {
            var result = new List<PatIRRemunerationValuationMatrixData>();
            if (!String.IsNullOrEmpty(matrixData.MatrixData))
            {
                var matrixDataItem = matrixData.MatrixData.Split("|");
                foreach (var item in matrixDataItem)
                {
                    var itemList = item.Split('~');
                    if (itemList[0].Equals("FactorId"))
                    {
                        PatIRRemunerationValuationMatrixData data = new PatIRRemunerationValuationMatrixData()
                        {
                            RemunerationId = matrixData.RemunerationId,
                            FactorId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2]),
                            UseManualEntry = options.Any(c => c.FactorManualId == Convert.ToInt32(itemList[1]) && c.ActualValue == 1)
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("MatrixId"))
                    {
                        PatIRRemunerationValuationMatrixData data = new PatIRRemunerationValuationMatrixData()
                        {
                            RemunerationId = matrixData.RemunerationId,
                            MatrixId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2]),
                            UseManualEntry = options.Any(c => c.MatrixManualId == Convert.ToInt32(itemList[1]) && c.ActualValue == 1)
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("CriteriaId"))
                    {
                        PatIRRemunerationValuationMatrixData data = new PatIRRemunerationValuationMatrixData()
                        {
                            RemunerationId = matrixData.RemunerationId,
                            CriteriaId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2]),
                            UseManualEntry = false
                        };
                        result.Add(data);
                    }
                }
            }

            return result;
        }

        private async Task<List<PatIRRemunerationValuationMatrixData>> CalculateMatrixData(List<PatIRRemunerationValuationMatrixData> initData)
        {
            var result = new List<PatIRRemunerationValuationMatrixData>();
            var criteria = initData.Where(c => c.CriteriaId != null).ToList();
            var matrixes = initData.Where(c => c.MatrixId != null).ToList();
            var matrixesResult = new List<PatIRRemunerationValuationMatrixData>();
            var factors = initData.Where(c => c.FactorId != null).ToList();
            var factorsResult = new List<PatIRRemunerationValuationMatrixData>();

            foreach (var matrixData in matrixes)
            {
                if (!matrixData.UseManualEntry)
                    matrixesResult.Add(await CalculateMatrixValue(matrixData, criteria));
                else
                    matrixesResult.Add(matrixData);
            }

            foreach (var factorData in factors)
            {
                if (!factorData.UseManualEntry)
                    factorsResult.Add(await CalculateFactorValue(factorData, matrixesResult));
                else
                    factorsResult.Add(factorData);
            }

            result.AddRange(criteria);
            result.AddRange(matrixesResult);
            result.AddRange(factorsResult);

            return result;
        }

        private async Task<PatIRRemunerationValuationMatrixData> CalculateMatrixValue(PatIRRemunerationValuationMatrixData matrixViewModel, List<PatIRRemunerationValuationMatrixData> criteriaViewModel)
        {
            var matrix = await _iRValuationMatrixService.QueryableList.FirstOrDefaultAsync(c => c.MatrixId == matrixViewModel.MatrixId);
            if (matrix == null)
                return matrixViewModel;

            var criteria = await _iRValuationMatrixCriteriaService.QueryableList.Where(c => c.MatrixId == matrixViewModel.MatrixId).ToListAsync();

            if (criteria == null)
                return matrixViewModel;

            var validCriteriaViewModel = criteriaViewModel.Where(c => criteria.Any(d => c.CriteriaId == d.CriteriaId));

            if (matrix.MatrixType == null)
                return matrixViewModel;

            if (matrix.MatrixType.Equals("Sum"))
            {
                double? result = 0;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue == 1)
                        result += criteria.FirstOrDefault(d => d.CriteriaId == c.CriteriaId).Value;
                });
                matrixViewModel.ActualValue = result;
            }
            else if (matrix.MatrixType.Equals("Count"))
            {
                double? result = 0;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue == 1)
                        result += 1;
                });
                matrixViewModel.ActualValue = result;
            }
            else if (matrix.MatrixType.Equals("Range"))
            {
                double? result = 0;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue == 1)
                        result += criteria.FirstOrDefault(d => d.CriteriaId == c.CriteriaId).Value;
                });
                matrixViewModel.ActualValue = result;
            }
            else if (matrix.MatrixType.Equals("Selection"))
            {
                double? result = 0;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue == 1)
                        result += criteria.FirstOrDefault(d => d.CriteriaId == c.CriteriaId).Value;
                });
                matrixViewModel.ActualValue = result;
            }
            else if (matrix.MatrixType.Equals("Max"))
            {
                double? result = double.MinValue;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue > result)
                        result = c.ActualValue;
                });
                matrixViewModel.ActualValue = result;
            }
            else if (matrix.MatrixType.Equals("Min"))
            {
                double? result = double.MaxValue;
                validCriteriaViewModel.ToList().ForEach(c =>
                {
                    if (c.ActualValue < result)
                        result = c.ActualValue;
                });
                matrixViewModel.ActualValue = result;
            }

            if (matrix.MaxValue != null && matrixViewModel.ActualValue > matrix.MaxValue)
                matrixViewModel.ActualValue = matrix.MaxValue;
            if (matrix.MinValue != null && matrixViewModel.ActualValue < matrix.MinValue)
                matrixViewModel.ActualValue = matrix.MinValue;

            return matrixViewModel;
        }

        private async Task<PatIRRemunerationValuationMatrixData> CalculateFactorValue(PatIRRemunerationValuationMatrixData factorViewModel, List<PatIRRemunerationValuationMatrixData> matrixViewModel)
        {
            var factor = await _iRFormulaFactorService.QueryableList.FirstOrDefaultAsync(c => c.FactorId == factorViewModel.FactorId);
            if (factor == null)
                return factorViewModel;

            var validMatrixViewModel = await _iRValuationMatrixDataService.QueryableList.Where(c => c.MatrixId != null && c.RemunerationId == factorViewModel.RemunerationId).ToListAsync();
            validMatrixViewModel.RemoveAll(c => matrixViewModel.Any(d => c.MatrixId == d.MatrixId));
            validMatrixViewModel.AddRange(matrixViewModel);
            var matrixes = await _iRValuationMatrixService.QueryableList.ToListAsync();

            string formula = factor.Formula;
            if (formula == null)
                return factorViewModel;

            foreach (var matrix in matrixes)
            {
                var currenctMatrixData = validMatrixViewModel.FirstOrDefault(c => c.MatrixId == matrix.MatrixId);
                double value = 0;
                if (currenctMatrixData != null)
                    value = currenctMatrixData.ActualValue ?? 1;
                else
                    value = matrix.DefaultValue ?? 1;
                formula = formula.Replace("{" + (string)matrix.Variable + "}", value.ToString());
            }

            formula = ReplaceVariableToOne(formula);

            if (String.IsNullOrEmpty(formula))
                return factorViewModel;

            DataTable dt = new DataTable();
            var result = dt.Compute(formula, "");
            factorViewModel.ActualValue = double.Parse(result.ToString());

            if (factor.MaxValue != null && factorViewModel.ActualValue > factor.MaxValue)
                factorViewModel.ActualValue = factor.MaxValue;
            if (factor.MinValue != null && factorViewModel.ActualValue < factor.MinValue)
                factorViewModel.ActualValue = factor.MinValue;

            return factorViewModel;
        }

        private string ReplaceVariableToOne(string formula)
        {
            string result = "";
            if (formula.Contains("{") && formula.Contains("}"))
            {
                int i = formula.IndexOf("{");
                int j = formula.IndexOf("}");
                result = formula.Substring(0, i);
                result += "1";
                result += formula.Substring(j);

                result = ReplaceVariableToOne(result);
            }
            return formula;
        }

        [Obsolete]
        public async Task<double> GetLumpSumAmount(int remunerationId, InventionInventorRemunerationInventorInfoViewModel invt)
        {
            var settings = _patSettings.GetSetting().Result;
            if (!(settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "Lump Sum"))
                return 0;

            double result = 0;
            var PercentageOfOwnerShip = await GetTotalInventorFactor(invt);
            var PercentageOfInvention = (invt.Percentage ?? 0) / 100;
            var PatIRProductSales = _patProductSaleService.QueryableList.Where(c => c.RemunerationId == remunerationId);
            var sales = PatIRProductSales.Where(c => c.Year == DateTime.Now.Year);

            var formulas = await _iRRemunerationFormulaService.QueryableList.Where(c => c.RemunerationType.Equals("Lump Sum")).OrderByDescending(c => c.EffStartDate).ToListAsync();
            var calculateDate = DateTime.Now;
            var formula = formulas.FirstOrDefault(c => (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate != null && c.EffEndDate >= calculateDate));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => ((c.EffStartDate == null) && (c.EffEndDate != null && c.EffEndDate >= calculateDate)) || (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate == null));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => c.EffStartDate == null && c.EffEndDate == null);

            var formulaText = "";
            if (formula != null)
                formulaText = formula.Formula ?? "";
            else
                return 0;

            formulaText = formulaText.Replace("{%ofOwnership}", PercentageOfOwnerShip.ToString());
            formulaText = formulaText.Replace("{%ofInvention}", PercentageOfInvention.ToString());
            formulaText = formulaText.Replace("{InitialPayment}", (invt.InitialPayment ?? 0).ToString());

            var formulaFactors = _iRFormulaFactorService.QueryableList.ToList();
            var formulaFactorData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FactorId != null && c.RemunerationId == invt.RemunerationId).ToList();

            foreach (var factor in formulaFactors)
            {
                var currenctfactorData = formulaFactorData.FirstOrDefault(c => c.FactorId == factor.FactorId);
                double value = 0;
                if (currenctfactorData != null)
                    value = currenctfactorData.ActualValue ?? 1;
                else
                    value = factor.DefaultValue ?? 1;

                if (!string.IsNullOrEmpty(factor.Variable))
                    formulaText = formulaText.Replace("{" + factor.Variable ?? "" + "}", value.ToString());
            }

            if (formulaText.Contains("{LicenseFactor}") || formulaText.Contains("{InventionValue}") || formulaText.Contains("{Revenue}"))
            {
                foreach (var sale in sales)
                {
                    double rate = 0;
                    if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                    {
                        rate = 1;
                    }
                    else
                    {
                        var er = _patEuroExchangeRateService.QueryableList.FirstOrDefault(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                        if (er == null)
                            throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);
                        if (er.UseDefault)
                        {
                            rate = er.DefaultExchangeRate;
                        }
                        else
                        {
                            var ery = _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefault(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                            if (ery == null)
                                throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                            rate = ery.ExchangeRate;
                        }
                    }

                    var formulaTextCopy = String.Copy(formulaText);
                    formulaTextCopy = formulaTextCopy.Replace("{LicenseFactor}", ((sale.LicenseFactor ?? 0) / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{InventionValue}", ((sale.InventionValue ?? 0) / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{Revenue}", ((sale.Revenue ?? 0) / rate).ToString());
                    formulaTextCopy = ReplaceVariableToOne(formulaTextCopy);
                    DataTable dt = new DataTable();
                    var ComputeResult = dt.Compute(formulaTextCopy, "");
                    result += double.Parse(ComputeResult.ToString());
                }
            }
            else
            {
                formulaText = ReplaceVariableToOne(formulaText);
                DataTable dt = new DataTable();
                var ComputeResult = dt.Compute(formulaText, "");
                result = double.Parse(ComputeResult.ToString());
            }

            if (formula != null)
            {
                if (formula.MaxValue != null && result > formula.MaxValue)
                    result = formula.MaxValue ?? 0;
                if (formula.MinValue != null && result < formula.MinValue)
                    result = formula.MinValue ?? 0;
            }

            return result.RoundTo2ndDecimals();
        }

        public async Task<InventionInventorRemunerationProductSaleStageInfoViewModel> GetStaggeredInfo(int productSaleId)
        {
            var model = new InventionInventorRemunerationProductSaleStageInfoViewModel();
            var productSale = await _patProductSaleService.GetByIdAsync(productSaleId);
            var productSales = await _patProductSaleService.QueryableList.Where(c => c.RemunerationId == productSale.RemunerationId).ToListAsync();

            model.CurrentProductSaleId = productSaleId;

            var salesThisYear = productSales.Where(c => c.Year == productSale.Year).ToList();
            var salesPreviousYear = productSales.Where(c => c.Year < productSale.Year).ToList();

            foreach (var sale in salesPreviousYear)
            {
                double rate = 0;
                if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                {
                    rate = 1;
                }
                else
                {
                    var er = await _patEuroExchangeRateService.QueryableList.FirstOrDefaultAsync(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                    if (er == null)
                        throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);

                    if (er.UseDefault)
                    {
                        rate = er.DefaultExchangeRate;
                    }
                    else
                    {
                        var ery = await _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefaultAsync(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                        if (ery == null)
                            throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                        rate = ery.ExchangeRate;
                    }
                }

                model.PrevRevenuesAccumulatedEuro += (sale.Revenue ?? 0) / rate;
            }
            model.CurrentRevenuesAccumulatedEuro = model.PrevRevenuesAccumulatedEuro;

            foreach (var sale in salesThisYear)
            {
                double rate = 0;
                if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                {
                    rate = 1;
                }
                else
                {
                    var er = await _patEuroExchangeRateService.QueryableList.FirstOrDefaultAsync(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                    if (er == null)
                        throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);

                    if (er.UseDefault)
                    {
                        rate = er.DefaultExchangeRate;
                    }
                    else
                    {
                        var ery = await _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefaultAsync(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                        if (ery == null)
                            throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                        rate = ery.ExchangeRate;
                    }
                }

                model.CurrentRevenuesAccumulatedEuro += (sale.Revenue ?? 0) / rate;
            }

            var staggering = await _patStaggeringEntityService.QueryableList.FirstOrDefaultAsync(s => s.IsActive == true);
            if (staggering == null)
                throw new NullReferenceException(_localizer["Staggering table is not found."]);

            var staggeringDetails = await _patStaggeringDetailService.QueryableList.Where(c => c.StaggeringId == staggering.StaggeringId).OrderBy(c => c.AmountFrom).ToListAsync();
            if (staggeringDetails == null)
                throw new NullReferenceException(_localizer["Staggering table is not found."]);

            var previousStaggeringDetails = staggeringDetails.Where(c => c.AmountFrom == null || c.AmountFrom < model.PrevRevenuesAccumulatedEuro).OrderBy(c => c.AmountFrom).ToList();
            var currentStaggeringDetails = staggeringDetails.Where(c => c.AmountFrom == null || c.AmountFrom < model.CurrentRevenuesAccumulatedEuro).OrderBy(c => c.AmountFrom).ToList();

            var previousStages = new List<InventionInventorRemunerationProductSaleStageInfoStageViewModel>();
            var currentStages = new List<InventionInventorRemunerationProductSaleStageInfoStageViewModel>();

            foreach (var d in previousStaggeringDetails)
            {
                var stage = new InventionInventorRemunerationProductSaleStageInfoStageViewModel();
                stage.Stage = d.Stage;
                stage.Amount = (double)((d.AmountTo == null || d.AmountTo >= model.PrevRevenuesAccumulatedEuro)
                    ? model.PrevRevenuesAccumulatedEuro - (d.AmountFrom ?? 0)
                    : (d.AmountTo ?? 0) - (d.AmountFrom ?? 0)
                    ) * (1 - d.Reduction);
                previousStages.Add(stage);
            }
            model.PrevStages = previousStages.AsQueryable();

            foreach (var d in currentStaggeringDetails)
            {
                var stage = new InventionInventorRemunerationProductSaleStageInfoStageViewModel();
                stage.Stage = d.Stage;
                stage.Amount = (double)((d.AmountTo == null || d.AmountTo >= model.CurrentRevenuesAccumulatedEuro)
                    ? model.CurrentRevenuesAccumulatedEuro - (d.AmountFrom ?? 0)
                    : (d.AmountTo ?? 0) - (d.AmountFrom ?? 0)
                    ) * (1 - d.Reduction);
                currentStages.Add(stage);
            }
            model.CurrentStages = currentStages.AsQueryable();

            model.PrevRevenuesStaggeredEuro = model.PrevStages.Sum(c => c.Amount);
            model.CurrentRevenuesStaggeredEuro = model.CurrentStages.Sum(c => c.Amount);
            model.RevenueForRemunerationEuro = model.CurrentRevenuesStaggeredEuro - model.PrevRevenuesStaggeredEuro;

            return model;
        }

        public async Task ProcessImportedProductSales(string userName)
        {
            var added = await _saleService.QueryableList.Where(c => (c.RecentAddedByImport ?? false)).ToListAsync();
            var updated = await _saleService.QueryableList.Where(c => (c.RecentUpdatedByImport ?? false)).ToListAsync();

            var settings = await _patSettings.GetSetting();
            var addedForProductSales = added.Where(c => c.Yr >= settings.ImportInventorRemunerationProductSalesCutOffYear).OrderByDescending(c => c.Yr).ToList();
            var updatedForProductSales = updated.Where(c => c.Yr >= settings.ImportInventorRemunerationProductSalesCutOffYear).OrderByDescending(c => c.Yr).ToList();

            try
            {
                var productIds = new List<int>();

                productIds.AddRange(addedForProductSales.Select(c => c.ProductId).Distinct().ToList());
                productIds.AddRange(updatedForProductSales.Select(c => c.ProductId).Distinct().ToList());

                await LoadInventionProductsByProductIds(productIds, userName);

            }
            catch (Exception e)
            {

            }
            finally
            {
                added.ForEach(c =>
                {
                    c.RecentAddedByImport = null;
                });
                updated.ForEach(c =>
                {
                    c.RecentUpdatedByImport = null;
                    c.CurrencyTypeBeforeImport = null;
                });
                added.AddRange(updated);
                await _saleService.Update(added);
            }
        }

        public async Task SharedProductSalesDelete(ProductSale deleted)
        {
            var patSettings = await _patSettings.GetSetting();
            var product = await _productService.QueryableList.FirstOrDefaultAsync(c => c.ProductId == deleted.ProductId);
            var IRSales = await _patProductSaleService.QueryableList.Where(c => c.Year == deleted.Yr &&
                (c.Country ?? "") == (deleted.Country ?? "") &&
                c.Product == product.ProductName &&
                ((c.CurrencyType ?? "") == (deleted.CurrencyType ?? ""))).ToListAsync();
            await _patProductSaleService.Delete(IRSales);
            await UpdateRemunerationUpdatedYears(IRSales);

            if (deleted.Net != 0 || string.IsNullOrEmpty(deleted.CurrencyType))
            {
                foreach (var IRsale in IRSales)
                {
                    try
                    {
                        await DeleteDistribution(IRsale);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        public async Task SharedProductSalesUpdate(List<ProductSale> updated, string userName)
        {
            var updatedSales = new List<PatIRProductSale>();
            var addedSales = new List<PatIRProductSale>();
            var deletedSales = new List<PatIRProductSale>();

            foreach (var update in updated.Where(s => s.ProductSaleBeforeUpdate != null && !string.IsNullOrEmpty(s.CurrencyType)))
            {
                var product = await _productService.QueryableList.FirstOrDefaultAsync(c => c.ProductId == update.ProductId);
                var productInvs = await _patProductInvService.QueryableList.Where(p => p.ProductId == update.ProductId).ToListAsync();

                foreach (var productInv in productInvs)
                {
                    var invention = await _inventionService.GetByIdAsync(productInv.InvId);
                    var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invention.InvId);
                    if (remuneration == null) continue;

                    var existingIRProductSale = await _patProductSaleService.QueryableList
                        .FirstOrDefaultAsync(c => c.RemunerationId == remuneration.RemunerationId &&
                                                    c.Year == update.ProductSaleBeforeUpdate.Yr &&
                                                    (c.Country ?? "") == (update.ProductSaleBeforeUpdate.Country ?? "") &&
                                                    c.Product == product.ProductName &&
                                                    (c.CurrencyType ?? "") == (update.ProductSaleBeforeUpdate.CurrencyType ?? ""));

                    if (existingIRProductSale != null)
                    {
                        if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > update.Yr)
                        {
                            deletedSales.Add(existingIRProductSale);
                            continue;
                        }

                        if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < update.Yr)
                        {
                            deletedSales.Add(existingIRProductSale);
                            continue;
                        }

                        existingIRProductSale.Year = update.Yr;
                        existingIRProductSale.Country = update.Country ?? "";
                        existingIRProductSale.CurrencyType = update.CurrencyType;
                        existingIRProductSale.LicenseFactor = productInv.LicenseFactor ?? 0;
                        existingIRProductSale.InventionValue = productInv.InventionValue ?? 0;

                        existingIRProductSale.Revenue = (double)update.Net;
                        existingIRProductSale.UseOverrideRevenue = false;
                        existingIRProductSale.LastUpdate = DateTime.Now;
                        existingIRProductSale.UpdatedBy = userName;

                        updatedSales.Add(existingIRProductSale);
                    }
                    else
                    {
                        if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > update.Yr)
                            continue;

                        if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < update.Yr)
                            continue;

                        PatIRProductSale productSale = new PatIRProductSale()
                        {
                            RemunerationId = remuneration.RemunerationId,
                            Product = product.ProductName,
                            Country = update.Country ?? "",
                            Year = update.Yr,
                            LicenseFactor = productInv.LicenseFactor ?? 0,
                            InventionValue = productInv.InventionValue ?? 0,

                            UnitPrice = 0,
                            Quantity = 0,
                            CurrencyType = update.CurrencyType,
                            Revenue = (double)update.Net,
                            UseOverrideRevenue = false,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            CreatedBy = userName,
                            UpdatedBy = userName,
                        };

                        addedSales.Add(productSale);
                    }
                }
            }

            foreach (var update in updated.Where(s => s.ProductSaleBeforeUpdate != null && string.IsNullOrEmpty(s.CurrencyType)))
            {
                var product = await _productService.QueryableList.FirstOrDefaultAsync(c => c.ProductId == update.ProductId);
                var productInvs = await _patProductInvService.QueryableList.Where(p => p.ProductId == update.ProductId).ToListAsync();

                foreach (var productInv in productInvs)
                {
                    var invention = await _inventionService.GetByIdAsync(productInv.InvId);
                    var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invention.InvId);
                    if (remuneration == null) continue;

                    var existingIRProductSale = await _patProductSaleService.QueryableList
                        .Where(c => c.RemunerationId == remuneration.RemunerationId &&
                                                    c.Year == update.ProductSaleBeforeUpdate.Yr &&
                                                    (c.Country ?? "") == (update.ProductSaleBeforeUpdate.Country ?? "") &&
                                                    c.Product == product.ProductName &&
                                                    (c.CurrencyType ?? "") == (update.ProductSaleBeforeUpdate.CurrencyType ?? "")).ToListAsync();

                    if (existingIRProductSale != null)
                        deletedSales.AddRange(existingIRProductSale);
                }
            }

            await _patProductSaleService.Add(addedSales);
            await _patProductSaleService.Update(updatedSales);
            await _patProductSaleService.Delete(deletedSales);
            var remunerationIds = updatedSales.Select(c => c.RemunerationId).Distinct().ToList();
            remunerationIds.AddRange(addedSales.Select(c => c.RemunerationId).Distinct().ToList());
            remunerationIds.AddRange(deletedSales.Select(c => c.RemunerationId).Distinct().ToList());
            remunerationIds = remunerationIds.Distinct().ToList();

            await UpdateRemunerationUpdatedYears(updatedSales);

            foreach (var remunerationId in remunerationIds)
            {
                try
                {
                    await UpdateDistribution(remunerationId, addedSales.Where(c => c.RemunerationId == remunerationId).ToList(), updatedSales.Where(c => c.RemunerationId == remunerationId).ToList(), deletedSales.Where(c => c.RemunerationId == remunerationId).ToList());
                }
                catch (Exception e)
                {

                }
            }
        }

        public async Task SharedProductSalesAdd(List<ProductSale> added, string userName)
        {
            var addedSales = new List<PatIRProductSale>();

            foreach (var add in added.Where(s => !string.IsNullOrEmpty(s.CurrencyType)))
            {
                var product = await _productService.QueryableList.FirstOrDefaultAsync(c => c.ProductId == add.ProductId);
                var productInvs = await _patProductInvService.QueryableList.Where(p => p.ProductId == add.ProductId).ToListAsync();
                foreach (var productInv in productInvs)
                {
                    var invention = await _inventionService.GetByIdAsync(productInv.InvId);
                    var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invention.InvId);
                    if (remuneration == null) continue;

                    if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > add.Yr)
                        continue;

                    if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < add.Yr)
                        continue;

                    if (!_patProductSaleService.QueryableList.Any(c => c.RemunerationId == remuneration.RemunerationId &&
                        c.Year == add.Yr &&
                        (c.Country ?? "") == (add.Country ?? "") &&
                        c.Product == product.ProductName &&
                        (c.CurrencyType ?? "") == (add.CurrencyType ?? "")))
                    {
                        PatIRProductSale productSale = new PatIRProductSale()
                        {
                            RemunerationId = remuneration.RemunerationId,
                            Product = product.ProductName,
                            Country = add.Country ?? "",
                            Year = add.Yr,
                            InventionValue = productInv.InventionValue ?? 0,
                            LicenseFactor = productInv.LicenseFactor ?? 0,
                            UnitPrice = 0,
                            Quantity = 0,
                            CurrencyType = add.CurrencyType,
                            Revenue = (double)add.Net,
                            UseOverrideRevenue = false,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            CreatedBy = userName,
                            UpdatedBy = userName,
                        };

                        addedSales.Add(productSale);
                    }
                }
            }

            await _patProductSaleService.Add(addedSales);
            await UpdateRemunerationUpdatedYears(addedSales);
            var remunerationIds = addedSales.Select(c => c.RemunerationId).Distinct().ToList();
            foreach (var remunerationId in remunerationIds)
            {
                try
                {
                    await UpdateDistribution(remunerationId, addedSales.Where(c => c.RemunerationId == remunerationId).ToList(), new List<PatIRProductSale>(), new List<PatIRProductSale>());
                }
                catch (Exception e)
                {

                }
            }
        }

        private async Task UpdateRemunerationUpdatedYears(List<PatIRProductSale> patIRProductSales)
        {
            var remunerationIds = patIRProductSales.Select(c => c.RemunerationId).Distinct().ToList();
            var remunerations = await _patRemunerationEntityService.QueryableList.ToListAsync();
            var updatedRemunerations = new List<PatIRRemuneration>();
            foreach (var remunerationId in remunerationIds)
            {
                var remuneration = remunerations.First(c => c.RemunerationId == remunerationId);
                var currentProductSalesUpdatedYears = (remuneration.ProductSalesUpdatedYears == null ? "" : remuneration.ProductSalesUpdatedYears);
                var currentYears = currentProductSalesUpdatedYears == "" ? new List<int>() : (currentProductSalesUpdatedYears.Contains(", ") ? currentProductSalesUpdatedYears.Split(", ").Select(Int32.Parse).ToList() : new List<int>() { int.Parse(currentProductSalesUpdatedYears) });
                var currentSaleYears = patIRProductSales.Where(c => c.RemunerationId == remunerationId).Select(c => c.Year).Cast<int>().ToList();
                currentYears.AddRange(currentSaleYears);
                currentYears = currentYears.Distinct().ToList();
                currentYears.Sort();
                var updatedProductSalesUpdatedYears = string.Join(", ", currentYears.ToArray());
                remuneration.ProductSalesUpdatedYears = updatedProductSalesUpdatedYears;
                updatedRemunerations.Add(remuneration);
            }
            await _patRemunerationEntityService.Update(updatedRemunerations);
        }

        public async Task LoadInventionProductsByProductIds(List<int> productIds, string userName)
        {
            if (productIds.Count > 0)
            {
                var productInvs = await _patProductInvService.QueryableList.Where(p => productIds.Contains(p.ProductId)).ToListAsync();
                var invIds = productInvs.Select(p => p.InvId).Distinct().ToList();

                foreach (var invId in invIds)
                {
                    var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == invId);
                    if (remuneration == null) continue;

                    await LoadInventionProducts(remuneration.InvId, remuneration.RemunerationId, userName);
                }
            }
        }

        public async Task LoadInventionProducts(int invId, int remunerationId, string userName)
        {
            var productInvs = await _patProductInvService.QueryableList.Where(p => p.InvId == invId).Include(p => p.Product).ToListAsync();
            var added = new List<PatIRProductSale>();
            var updated = new List<PatIRProductSale>();
            var existingIRProductSales = await _patProductSaleService.QueryableList.Where(s => s.RemunerationId == remunerationId).ToListAsync();
            var deleted = existingIRProductSales.Where(s => !productInvs.Any(p => p.Product.ProductName == s.Product)).ToList();

            foreach (var productInv in productInvs)
            {
                var sales = await _saleService.QueryableList.Where(s => s.ProductId == productInv.ProductId).ToListAsync();

                var deletedSales = existingIRProductSales.Where(s => s.Product == productInv.Product.ProductName
                                    && !sales.Any(sale => sale.Yr == s.Year
                                    && sale.CurrencyType == s.CurrencyType
                                    && (s.Country ?? "") == (sale.Country ?? ""))).ToList();

                if (deletedSales.Any())
                    deleted.AddRange(deletedSales);

                foreach (var sale in sales.Where(s => !string.IsNullOrEmpty(s.CurrencyType)))
                {
                    var existingIRProductSale = existingIRProductSales
                            .Where(s => s.Product == productInv.Product.ProductName
                                    && s.Year == sale.Yr
                                && s.CurrencyType == sale.CurrencyType
                                    && (s.Country ?? "") == (sale.Country ?? "")
                                    ).FirstOrDefault();

                    if (existingIRProductSale == null)
                    {
                        if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > sale.Yr)
                            continue;
                        if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < sale.Yr)
                            continue;

                        var irProductSale = new PatIRProductSale
                        {
                            RemunerationId = remunerationId,
                            Product = productInv.Product.ProductName,
                            Year = sale.Yr,
                            Country = sale.Country,
                            LicenseFactor = productInv.LicenseFactor ?? 0,
                            InventionValue = productInv.InventionValue ?? 0,

                            Revenue = (double)sale.Net,
                            UseOverrideRevenue = false,
                            CurrencyType = sale.CurrencyType,
                            CreatedBy = userName,
                            UpdatedBy = userName,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now
                        };

                        added.Add(irProductSale);
                    }
                    else
                    {
                        if (productInv.StartDate != null && ((DateTime)productInv.StartDate).Year > sale.Yr)
                        {
                            deletedSales.Add(existingIRProductSale);
                            continue;
                        }

                        if (productInv.EndDate != null && ((DateTime)productInv.EndDate).Year < sale.Yr)
                        {
                            deletedSales.Add(existingIRProductSale);
                            continue;
                        }

                        existingIRProductSale.LicenseFactor = productInv.LicenseFactor ?? 0;
                        existingIRProductSale.InventionValue = productInv.InventionValue ?? 0;

                        existingIRProductSale.Revenue = (double)sale.Net;
                        existingIRProductSale.UseOverrideRevenue = false;
                        existingIRProductSale.UpdatedBy = userName;
                        existingIRProductSale.LastUpdate = DateTime.Now;

                        updated.Add(existingIRProductSale);
                    }
                }
            }

            if (updated.Any() || added.Any() || deleted.Any())
            {
                try
                {
                    await _patProductSaleService.Update(remunerationId, userName,
                                        _mapper.Map<List<PatIRProductSale>>(updated),
                                        _mapper.Map<List<PatIRProductSale>>(added),
                                        _mapper.Map<List<PatIRProductSale>>(deleted)
                                        );

                    await UpdateDistribution(remunerationId, added, updated, deleted);
                }
                catch (Exception ex)
                {
                    var err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }
        }

        public async Task<List<RemunerationRevenueViewModel>> GetRemunerationRevenue(int InvId)
        {
            var result = new List<RemunerationRevenueViewModel>();

            var remuneration = await _patRemunerationEntityService.QueryableList.FirstOrDefaultAsync(c => c.InvId == InvId);
            if (remuneration == null)
                return result;

            var patIRProductSales = await _patProductSaleService.QueryableList.Where(c => c.RemunerationId == remuneration.RemunerationId).ToListAsync();
            if (patIRProductSales.Count == 0)
                return result;

            var staggering = await _patStaggeringEntityService.QueryableList.FirstOrDefaultAsync(c => c.IsActive == true);
            if (staggering == null)
                return result;

            var staggeringDtl = await _patStaggeringDetailService.QueryableList.Where(c => c.StaggeringId == staggering.StaggeringId).OrderBy(c => c.AmountFrom ?? 0).ToListAsync();
            if (staggeringDtl == null)
                return result;

            var years = patIRProductSales.Select(c => c.Year ?? 0).Distinct().ToList();
            double revernuesInAYear = 0;
            double revenuesAccumulatedEuro = 0;
            double processedAmount = 0;

            foreach (var year in years.OrderBy(x => x))
            {
                revernuesInAYear = 0;
                foreach (var sale in patIRProductSales.Where(c => c.Year == year))
                {
                    double rate = 0;
                    if ((sale.CurrencyType ?? "").ToLower() == "EUR".ToLower())
                    {
                        rate = 1;
                    }
                    else
                    {
                        var er = await _patEuroExchangeRateService.QueryableList.FirstOrDefaultAsync(c => c.CurrencyType.ToLower() == (sale.CurrencyType ?? "").ToLower());
                        if (er == null)
                            throw new NullReferenceException((sale.CurrencyType ?? "") + " " + _localizer["exchange rate is not found."]);

                        if (er.UseDefault)
                        {
                            rate = er.DefaultExchangeRate;
                        }
                        else
                        {
                            var ery = await _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefaultAsync(c => c.ExchangeId == er.ExchangeId && c.Year == sale.Year);
                            if (ery == null)
                                throw new NullReferenceException(sale.CurrencyType + " " + _localizer["exchange rate is not found for the year"] + " " + sale.Year + ". " + _localizer["Please add the exchange rate first."]);
                            rate = ery.ExchangeRate;
                        }
                    }

                    revernuesInAYear += (sale.Revenue ?? 0) / rate;
                }

                revenuesAccumulatedEuro += revernuesInAYear;


                foreach (var stage in staggeringDtl.Where(c => (c.AmountTo ?? double.MaxValue) < revenuesAccumulatedEuro).OrderBy(c => c.AmountFrom ?? 0))
                {
                    var step = new RemunerationRevenueViewModel()
                    {
                        Year = year,
                        Stage = stage.AmountTo ?? 999999999999,
                        Revenue = (stage.AmountTo ?? double.MaxValue) - processedAmount,
                        Reduction = stage.Reduction,
                        ReducedRevenue = (stage.AmountTo - processedAmount) * (1 - stage.Reduction)
                    };

                    result.Add(step);

                    processedAmount = stage.AmountTo ?? double.MaxValue;
                }

                staggeringDtl.RemoveAll(c => (c.AmountTo ?? double.MaxValue) < revenuesAccumulatedEuro);

                if (staggeringDtl.Any())
                {
                    var lastStep = new RemunerationRevenueViewModel()
                    {
                        Year = year,
                        Stage = staggeringDtl.First().AmountTo ?? 999999999999,
                        Revenue = revenuesAccumulatedEuro - processedAmount,
                        Reduction = staggeringDtl.First().Reduction,
                        ReducedRevenue = (revenuesAccumulatedEuro - processedAmount) * (1 - staggeringDtl.First().Reduction)
                    };

                    result.Add(lastStep);

                    processedAmount = revenuesAccumulatedEuro;
                }
                else
                    throw new NullReferenceException($"Total revenues exceed the final stage in the {staggering.Name} staggering table. Please review the staggering table.");

            }

            return result;
        }
    }
}
