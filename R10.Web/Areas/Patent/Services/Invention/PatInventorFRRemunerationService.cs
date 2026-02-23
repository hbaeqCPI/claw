using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Web.Interfaces;
using R10.Web.Areas.Patent.ViewModels;
using R10.Core.Interfaces.Patent;
using AutoMapper;
using System.Data;
using R10.Web.Extensions;
using R10.Core.Entities;
using R10.Web.Models;
using Microsoft.Extensions.Localization;
using ActiveQueryBuilder.View.DatabaseSchemaView;

namespace R10.Web.Areas.Patent.Services
{
    public class PatInventorFRRemunerationService : IPatInventorFRRemunerationService
    {
        private readonly IChildEntityService<PatIRFRRemuneration, PatInventorInv> _patInventorInvService;
        private readonly IChildEntityService<PatIRFRRemuneration, PatIRFRProductSale> _patProductSaleService;
        private readonly IEntityService<PatIRFRDistribution> _patDistributionEntityService;
        private readonly IPatInventorService _patInventorService;
        //private readonly IEntityService<PatIRFREmployeePosition> _patEmployeePositionService;
        private readonly IEntityService<PatIRFRValorizationRule> _patValorizationRuleEntityService;
        private readonly IEntityService<PatIRFRRemuneration> _patRemunerationEntityService;
        private readonly IEntityService<PatIRFRStaggering> _patStaggeringEntityService;
        private readonly IChildEntityService<PatIRFRStaggering, PatIRFRStaggeringDetail> _patStaggeringDetailService;
        private readonly IEntityService<PatIREuroExchangeRate> _patEuroExchangeRateService;
        private readonly IChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly> _patEuroExchangeRateYearlyService;
        private readonly ICountryApplicationService _applicationService;
        private readonly IProductService _productService;
        private readonly IInventionService _inventionService;
        private readonly IEntityService<PatIRFRRemunerationFormula> _iRRemunerationFormulaService;
        private readonly IEntityService<PatIRFRRemunerationFormulaFactor> _iRFormulaFactorService;
        private readonly IEntityService<PatIRFRRemunerationValuationMatrixType> _iRValuationMatrixTypeService;
        private readonly IEntityService<PatIRFRRemunerationValuationMatrix> _iRValuationMatrixService;
        private readonly IChildEntityService<PatIRFRRemunerationValuationMatrix, PatIRFRRemunerationValuationMatrixCriteria> _iRValuationMatrixCriteriaService;
        private readonly IChildEntityService<PatIRFRRemuneration, PatIRFRRemunerationValuationMatrixData> _iRValuationMatrixDataService;
        private readonly IProductSaleService _saleService;
        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly IStringLocalizer<InventionResource> _localizer;

        public PatInventorFRRemunerationService(
            IChildEntityService<PatIRFRRemuneration, PatInventorInv> patInventorInvService,
            IChildEntityService<PatIRFRRemuneration, PatIRFRProductSale> patProductSaleService,
            IEntityService<PatIRFRDistribution> patDistributionEntityService,
            IPatInventorService patInventorService,
            //IEntityService<PatIRFREmployeePosition> patEmployeePositionService,
            IEntityService<PatIRFRValorizationRule> patValorizationRuleEntityService,
            IEntityService<PatIRFRRemuneration> patRemunerationEntityService,
            IEntityService<PatIRFRStaggering> patStaggeringEntityService,
            IChildEntityService<PatIRFRStaggering, PatIRFRStaggeringDetail> patStaggeringDetailService,
            IEntityService<PatIREuroExchangeRate> patEuroExchangeRateService,
            IChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly> patEuroExchangeRateYearlyService,
            ICountryApplicationService applicationService,
            IProductService productService,
            IInventionService inventionService,
            IEntityService<PatIRFRRemunerationFormula> iRRemunerationFormulaService,
            IEntityService<PatIRFRRemunerationFormulaFactor> iRFormulaFactorService,
            IEntityService<PatIRFRRemunerationValuationMatrixType> iRValuationMatrixTypeService,
            IEntityService<PatIRFRRemunerationValuationMatrix> iRValuationMatrixService,
            IChildEntityService<PatIRFRRemunerationValuationMatrix, PatIRFRRemunerationValuationMatrixCriteria> iRValuationMatrixCriteriaService,
            IChildEntityService<PatIRFRRemuneration, PatIRFRRemunerationValuationMatrixData> iRValuationMatrixDataService,
            IProductSaleService saleService,
            IMapper mapper,
            ISystemSettings<PatSetting> patSettings,
            IStringLocalizer<InventionResource> localizer
            )
        {
            _patInventorInvService = patInventorInvService;
            _patProductSaleService = patProductSaleService;
            _patDistributionEntityService = patDistributionEntityService;
            _patInventorService = patInventorService;
            //_patEmployeePositionService = patEmployeePositionService;
            _patValorizationRuleEntityService = patValorizationRuleEntityService;
            _patRemunerationEntityService = patRemunerationEntityService;
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
            _mapper = mapper;
            _patSettings = patSettings;
            _localizer = localizer;
        }

        public async Task UpdateDistribution(int remunerationId, List<PatIRFRProductSale> added, List<PatIRFRProductSale> updated)
        {
            var earliestUpdatedYear = (added.Min(c => c.Year) ?? int.MaxValue) < (updated.Min(c => c.Year) ?? int.MaxValue) ? added.Min(c => c.Year) : updated.Min(c => c.Year);
            await UpdateDistributionAfterYear(remunerationId, earliestUpdatedYear ?? 9999);
        }

        public async Task UpdateDistributionAfterYear(int remunerationId, int currentYear)
        {
            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.FRRemunerationId == remunerationId && !c.PaidByLumpSum).ToList(); //&& c.InventorInvInventor.Citizenship != null && c.InventorInvInventor.Citizenship.ToUpper().Equals("DE") && !c.PaidByLumpSum);
            var PatIRFRProductSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == remunerationId).ToList();
            var PatIRFRDistributions = _patDistributionEntityService.QueryableList.ToList();

            List<int> updatedYears = PatIRFRProductSales.Where(c => c.Year >= currentYear).Select(c => (int)c.Year).Distinct().ToList();
            if (!updatedYears.Contains(currentYear))
                updatedYears.Add(currentYear);

            List<PatIRFRDistribution> addedDistributions = new List<PatIRFRDistribution>();
            List<PatIRFRDistribution> updatedDistributions = new List<PatIRFRDistribution>();
            List<PatIRFRDistribution> deletedDistributions = new List<PatIRFRDistribution>();

            foreach (int year in updatedYears)
            {
                foreach (PatInventorInv invt in PatInventorInvs)
                {
                    var distribution = PatIRFRDistributions.FirstOrDefault(c => c.InventorInvID == invt.InventorInvID && c.Year == year);
                    if (distribution == null)
                    {
                        if (PatIRFRProductSales.Any(c => c.Year == year))
                        {
                            PatIRFRDistribution add = new PatIRFRDistribution
                            {
                                InventorInvID = invt.InventorInvID,
                                Year = year,
                                Amount = await CalculateDistributionValue(year, PatIRFRProductSales, invt)
                            };
                            addedDistributions.Add(add);
                        }
                    }
                    else if (distribution.PaidDate == null)
                    {
                        if (PatIRFRProductSales.Any(c => c.Year == year))
                        {
                            if (!distribution.UseOverrideAmount)
                                distribution.Amount = await CalculateDistributionValue(year, PatIRFRProductSales, invt); ;
                            updatedDistributions.Add(distribution);
                        }
                        else
                        {
                            deletedDistributions.Add(distribution);
                        }
                    }
                }
            }

            await _patDistributionEntityService.Add(addedDistributions);
            await _patDistributionEntityService.Update(updatedDistributions);
            await _patDistributionEntityService.Delete(deletedDistributions);
        }

        private async Task<double> CalculateDistributionValue(int year, List<PatIRFRProductSale> productSales, PatInventorInv invt)
        {
            double result = 0;
            var patSettings = await _patSettings.GetSetting();
            var PercentageOfOwnerShip = GetTotalInventorFactor(invt);
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
            var formulaFactorData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FactorId != null && c.FRRemunerationId == invt.FRRemunerationId).ToList();

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

            if (formulaText.Contains("{LicenseFactor}") || formulaText.Contains("{Revenue}") || formulaText.Contains("{RevenueForRemuneration}"))
            {
                RevernueForRemuneration revenueForRemuneration = new RevernueForRemuneration();
                if (formulaText.Contains("{RevenueForRemuneration}"))
                {
                    revenueForRemuneration = GetRevenueForRemuneration(year, productSales, (int)invt.FRRemunerationId);
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
                    formulaTextCopy = formulaTextCopy.Replace("{LicenseFactor}", (sale.LicenseFactor / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{Revenue}", ((sale.Revenue ?? 0) / rate).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{RevenueForRemuneration}", (revenueForRemuneration.RevenueForRemuneration * ((sale.Revenue ?? 0) / rate) * (sale.InventionValue ?? 0) / 100 / (revenueForRemuneration.TotalRevenueThisYear == 0 ? 1 : revenueForRemuneration.TotalRevenueThisYear)).ToString());
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

        public async Task<double> CalculateDistributionValue(PatIRFRDistribution distribution)
        {
            int year = (int)distribution.Year;
            var invt = _patInventorInvService.QueryableList.First(c => c.InventorInvID == distribution.InventorInvID);
            var PatIRFRProductSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == invt.FRRemunerationId && c.Year <= year).ToList();
            return await CalculateDistributionValue(year, PatIRFRProductSales, invt);
        }

        private RevernueForRemuneration GetRevenueForRemuneration(int year, List<PatIRFRProductSale> productSales, int remunerationId)
        {
            RevernueForRemuneration rfr = new RevernueForRemuneration() { RevenueForRemuneration = 0, TotalRevenueThisYear = 0 };
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

                rfr.TotalRevenueThisYear += ((sale.Revenue ?? 0) / rate) * (sale.InventionValue ?? 0) / 100;
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

                TotalRevenuePreviousYear += ((sale.Revenue ?? 0) / rate) * (sale.InventionValue ?? 0) / 100;
            }
            PatIRFRStaggering staggering = _patStaggeringEntityService.QueryableList.Where(c => c.Year <= year).OrderByDescending(c => c.Year).First();
            double exchangeRate = 0;
            var erDM = _patEuroExchangeRateService.QueryableList.FirstOrDefault(c => c.CurrencyType.ToLower() == "DM".ToLower());
            if (erDM == null)
                throw new NullReferenceException("DM " + _localizer["exchange rate is not found."]);
            if (erDM.UseDefault)
            {
                exchangeRate = erDM.DefaultExchangeRate;
            }
            else
            {
                var ery = _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefault(c => c.ExchangeId == erDM.ExchangeId && c.Year == year);
                if (ery == null)
                    throw new NullReferenceException("DM " + _localizer["exchange rate is not found for the year"] + " " + year + ". " + _localizer["Please add the exchange rate first."]);
                exchangeRate = ery.ExchangeRate;
            }
            IQueryable<PatIRFRStaggeringDetail> staggeringDetails = _patStaggeringDetailService.QueryableList.Where(c => c.StaggeringId == staggering.StaggeringId).OrderBy(c => c.AmountFrom);
            if (staggeringDetails.Count() == 0)
            {
                rfr.RevenueForRemuneration = rfr.TotalRevenueThisYear;
            }
            else
            {
                var totalRevenuePreviousYearInDM = TotalRevenuePreviousYear * exchangeRate;
                var totalRevenueToThisYearInDM = rfr.TotalRevenueThisYear * exchangeRate + totalRevenuePreviousYearInDM;
                staggeringDetails = staggeringDetails.Where(c =>
                (c.AmountTo == null || c.AmountTo >= totalRevenuePreviousYearInDM) &&
                (c.AmountFrom == null || c.AmountFrom < totalRevenueToThisYearInDM)).OrderBy(c => c.AmountFrom).OrderBy(c => c.AmountFrom);
                double revenueForRemunerationInDM = 0;
                foreach (var detail in staggeringDetails)
                {
                    /// F from T to P previous W revenue to this year
                    // F P
                    if (detail.AmountFrom == null || totalRevenuePreviousYearInDM > detail.AmountFrom)
                    {
                        // F P W T
                        if (detail.AmountTo == null || totalRevenueToThisYearInDM <= detail.AmountTo)
                        {
                            revenueForRemunerationInDM += (totalRevenueToThisYearInDM - totalRevenuePreviousYearInDM) * (1 - detail.Reduction);
                        }
                        else //F P T W
                        {
                            revenueForRemunerationInDM += ((detail.AmountTo ?? 0) - totalRevenuePreviousYearInDM) * (1 - detail.Reduction);
                        }
                    }
                    else // P F
                    {
                        // P F W T
                        if (detail.AmountTo == null || totalRevenueToThisYearInDM <= detail.AmountTo)
                        {
                            revenueForRemunerationInDM += (totalRevenueToThisYearInDM - (detail.AmountFrom ?? 0)) * (1 - detail.Reduction);
                        }
                        else //P F T W
                        {
                            revenueForRemunerationInDM += ((detail.AmountTo ?? 0) - (detail.AmountFrom ?? 0)) * (1 - detail.Reduction);
                        }
                    }
                }

                rfr.RevenueForRemuneration = revenueForRemunerationInDM / exchangeRate;
            }

            return rfr;
        }

        private class RevernueForRemuneration
        {
            public double RevenueForRemuneration { get; set; }
            public double TotalRevenueThisYear { get; set; }
        }

        public async Task DeleteDistribution(PatIRFRProductSale deleted)
        {
            await UpdateDistributionAfterYear(deleted.FRRemunerationId, (int)deleted.Year);
        }

        // not in use
        public async Task UpdateDistribution(int remunerationId, List<PatInventorInv> updated)
        {
            var PatIRFRProductSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == remunerationId).ToList();
            var PatIRFRDistributions = _patDistributionEntityService.QueryableList.ToList();

            List<int> updatedYears = new List<int>();
            foreach (var patIRFRProductSale in PatIRFRProductSales)
            {
                updatedYears.Add((int)patIRFRProductSale.Year);
            }
            updatedYears = updatedYears.Distinct().ToList();

            List<PatIRFRDistribution> addedDistributions = new List<PatIRFRDistribution>();
            List<PatIRFRDistribution> deletedDistributions = new List<PatIRFRDistribution>();
            List<PatIRFRDistribution> updatedDistributions = new List<PatIRFRDistribution>();

            foreach (int year in updatedYears)
            {
                foreach (PatInventorInv invt in updated)
                {
                    if (invt.PaidByLumpSum)
                    {
                        deletedDistributions.AddRange(PatIRFRDistributions.Where(c => c.InventorInvID == invt.InventorInvID && c.Year == year && c.PaidDate == null));
                    }
                    else
                    {
                        var distribution = PatIRFRDistributions.FirstOrDefault(c => c.InventorInvID == invt.InventorInvID && c.Year == year);
                        if (distribution == null)
                        {
                            PatIRFRDistribution add = new PatIRFRDistribution
                            {
                                InventorInvID = invt.InventorInvID,
                                Year = year,
                                Amount = await CalculateDistributionValue(year, PatIRFRProductSales, invt)
                            };
                            addedDistributions.Add(add);
                        }
                        else if (distribution.PaidDate == null)
                        {
                            if (!distribution.UseOverrideAmount)
                                distribution.Amount = await CalculateDistributionValue(year, PatIRFRProductSales, invt);
                            updatedDistributions.Add(distribution);
                        }
                    }
                }
            }

            await _patDistributionEntityService.Add(addedDistributions);
            await _patDistributionEntityService.Update(updatedDistributions);
            await _patDistributionEntityService.Delete(deletedDistributions);
        }

        private double GetTotalInventorFactor(PatInventorInv invt)
        {
            return (double)(CalculateInventorPosition(((invt.PositionA ?? 0) + (invt.PositionB ?? 0) + (invt.PositionC ?? 0)))) / 100;
        }

        public InventionInventorRemunerationTotalCostViewModel GetInventorRemunerationTotalCost(int invId)
        {
            var invention = _inventionService.QueryableList.FirstOrDefault(i => i.InvId == invId);
            if (invention == null)
                return new InventionInventorRemunerationTotalCostViewModel();
            InventionInventorRemunerationTotalCostViewModel viewModel = new InventionInventorRemunerationTotalCostViewModel()
            {
                InvId = invention.InvId,
                CaseNumber = invention.CaseNumber,
                TotalCost = GetTotalCost(invention),
                YearlyCost = GetYearlyCost(invention),
                Module = "French"
            };

            return viewModel;
        }

        public InventionInventorRemunerationTotalCostViewModel GetInventorRemunerationTotalCost(Invention invention)
        {
            //var invention = _inventionService.QueryableList.FirstOrDefault(i => i.InvId == invId);
            if (invention == null)
                return new InventionInventorRemunerationTotalCostViewModel();
            InventionInventorRemunerationTotalCostViewModel viewModel = new InventionInventorRemunerationTotalCostViewModel()
            {
                InvId = invention.InvId,
                CaseNumber = invention.CaseNumber,
                TotalCost = GetTotalCost(invention),
                YearlyCost = GetYearlyCost(invention),
                Module = "French"
            };

            return viewModel;
        }

        public double GetTotalCost(Invention invention)
        {
            double totalCost = 0;
            //var inventors = _patInventorInvService.QueryableList.Where(i => i.InvId == invention.InvId);

            //foreach (var inventor in inventors)
            //{
            //    var distribution = _patDistributionEntityService.QueryableList.Where(c => c.InventorInvID == inventor.InventorInvID);
            //    totalCost += distribution.Sum(c => (c.Amount));
            //}

            var inventorInvs = _patInventorInvService.QueryableList.Where(c => c.InvId == invention.InvId && ((c.FRFirstPayment != null && c.FRFirstPaymentDate != null) || (c.FRSecondPayment != null && c.FRSecondPaymentDate != null) || (c.FRThirdPayment != null && c.FRThirdPaymentDate != null)));

            foreach (var inventorInv in inventorInvs)
            {
                totalCost += inventorInv.FRFirstPaymentDate != null ? (inventorInv.FRFirstPayment ?? 0) : 0;
                totalCost += inventorInv.FRSecondPaymentDate != null ? (inventorInv.FRSecondPayment ?? 0) : 0;
                totalCost += inventorInv.FRThirdPaymentDate != null ? (inventorInv.FRThirdPayment ?? 0) : 0;
            }

            return totalCost;
        }

        public List<InventionInventorRemunerationYearlyCostViewModel> GetYearlyCost(Invention invention)
        {
            List<InventionInventorRemunerationYearlyCostViewModel> YearlyCost = new List<InventionInventorRemunerationYearlyCostViewModel>();
            //var inventors = _patInventorInvService.QueryableList.Where(i => i.InvId == invention.InvId);

            //var distributions = _patDistributionEntityService.QueryableList.Where(c => inventors.Any(i => i.InventorInvID == c.InventorInvID));
            //var years = distributions.Select(c => new { Year = c.Year }).OrderBy(c => c.Year).Distinct();

            //foreach (var year in years)
            //{
            //    var distribution = distributions.Where(c => c.Year == year.Year);
            //    InventionInventorRemunerationYearlyCostViewModel viewModel = new InventionInventorRemunerationYearlyCostViewModel()
            //    {
            //        Year = (int)year.Year,
            //        Cost = distribution.Sum(c => c.Amount)
            //    };

            //    YearlyCost.Add(viewModel);
            //}

            var inventorInvs = _patInventorInvService.QueryableList.Where(c => c.InvId == invention.InvId && ((c.FRFirstPayment != null && c.FRFirstPaymentDate != null) || (c.FRSecondPayment != null && c.FRSecondPaymentDate != null) || (c.FRThirdPayment != null && c.FRThirdPaymentDate != null)));


            foreach (var inventorInv in inventorInvs)
            {
                if (inventorInv.FRFirstPaymentDate != null)
                {
                    int year = ((DateTime)inventorInv.FRFirstPaymentDate).Year;
                    if (YearlyCost.Where(c => c.Year == year).Count() > 0)
                    {
                        YearlyCost.FirstOrDefault(c => c.Year == year).Cost += inventorInv.FRFirstPayment ?? 0;
                    }
                    else
                    {
                        InventionInventorRemunerationYearlyCostViewModel viewModel = new InventionInventorRemunerationYearlyCostViewModel()
                        {
                            Year = year,
                            Cost = inventorInv.FRFirstPayment ?? 0
                        };

                        YearlyCost.Add(viewModel);
                    }
                }
                if (inventorInv.FRSecondPaymentDate != null)
                {
                    int year = ((DateTime)inventorInv.FRSecondPaymentDate).Year;
                    if (YearlyCost.Where(c => c.Year == year).Count() > 0)
                    {
                        YearlyCost.FirstOrDefault(c => c.Year == year).Cost += inventorInv.FRSecondPayment ?? 0;
                    }
                    else
                    {
                        InventionInventorRemunerationYearlyCostViewModel viewModel = new InventionInventorRemunerationYearlyCostViewModel()
                        {
                            Year = year,
                            Cost = inventorInv.FRSecondPayment ?? 0
                        };

                        YearlyCost.Add(viewModel);
                    }
                }
                if (inventorInv.FRThirdPaymentDate != null)
                {
                    int year = ((DateTime)inventorInv.FRThirdPaymentDate).Year;
                    if (YearlyCost.Where(c => c.Year == year).Count() > 0)
                    {
                        YearlyCost.FirstOrDefault(c => c.Year == year).Cost += inventorInv.FRThirdPayment ?? 0;
                    }
                    else
                    {
                        InventionInventorRemunerationYearlyCostViewModel viewModel = new InventionInventorRemunerationYearlyCostViewModel()
                        {
                            Year = year,
                            Cost = inventorInv.FRThirdPayment ?? 0
                        };

                        YearlyCost.Add(viewModel);
                    }
                }
            }

            return YearlyCost;
        }

        public int CalculateInventorPosition(int? sum)
        {
            if (sum == null)
                return 0;
            var valorizationRule = _patValorizationRuleEntityService.QueryableList.FirstOrDefault(c => (int)c.Point == (int)sum);
            if (valorizationRule == null)
            {
                return 0;
            }
            else
            {
                return valorizationRule.Ratio ?? 0;
            }
        }

        public async Task InitRemuneration(Invention invention)
        {
            if (invention.InvId > 0)
            {
                if (!_patRemunerationEntityService.QueryableList.Any(c => c.InvId == invention.InvId))
                {
                    //Create Remuneration record
                    PatIRFRRemuneration remuneration = new PatIRFRRemuneration()
                    {
                        InvId = invention.InvId,
                        CreatedBy = invention.UpdatedBy,
                        DateCreated = DateTime.Now,
                        UpdatedBy = invention.UpdatedBy,
                        LastUpdate = DateTime.Now
                    };
                    await _patRemunerationEntityService.Add(remuneration);

                    var settings = _patSettings.GetSetting().Result;
                    var noOfInvetorsToAward = settings.InventorFRRemunerationNoInventors;
                    if (noOfInvetorsToAward == 0)
                        noOfInvetorsToAward = int.MaxValue;
                    //Update Inventors to equal share
                    var PatInventorInvs = GetAvaliablePatInventorInvs(invention.InvId).OrderBy(ivtr => ivtr.OrderOfEntry).ToList();
                    PatInventorInvs.ForEach(itr => { itr.FRRemunerationId = remuneration.FRRemunerationId; });
                    await _patInventorInvService.Update(remuneration.FRRemunerationId, invention.UpdatedBy, PatInventorInvs, new List<PatInventorInv>() { }, new List<PatInventorInv>() { });

                    if (!PatInventorInvs.Any(d => d.Percentage > 0))
                    {
                        var count = PatInventorInvs.Count() > noOfInvetorsToAward ? noOfInvetorsToAward : PatInventorInvs.Count();
                        if (count == 0)
                            count = 1;

                        double equalSharePercentage = (double)(100.0 / count).RoundTo2ndDecimals();

                        //var inventors = _patInventorService.QueryableList;
                        //var inventorPositions = _patEmployeePositionService.QueryableList;

                        int orderOfEntry = 1;

                        PatInventorInvs.ForEach(c =>
                        {
                            c.Percentage = 0;
                            c.FRRemunerationId = remuneration.FRRemunerationId;
                            c.InventorInvInvention = null;
                            //var inventor = inventors.FirstOrDefault(d => d.InventorID == c.InventorID);
                            //c.PositionId = inventor == null ? null : inventor.PositionId;
                            //var inventionPosition = inventorPositions.FirstOrDefault(d => d.PositionId == c.PositionId);
                            //c.PositionA = inventionPosition == null ? null : inventionPosition.PositionA;
                            //c.PositionB = inventionPosition == null ? null : inventionPosition.PositionB;
                            //c.PositionC = inventionPosition == null ? null : inventionPosition.PositionC;

                            if (orderOfEntry <= noOfInvetorsToAward)
                            {
                                var v = _mapper.Map<InventionInventorFRRemunerationInventorInfoViewModel>(c);
                                c.FRThirdPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "In Use Award");
                                c.FRSecondPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "First Filing Award");
                                c.FRFirstPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "Intial Award");
                            }

                            orderOfEntry++;
                        });

                        if (PatInventorInvs.Count() != 0)
                            PatInventorInvs.OrderBy(c => c.OrderOfEntry).Take(count).ToList().ForEach(c =>
                            {
                                c.Percentage = equalSharePercentage;
                            });

                        await _patInventorInvService.Update(remuneration.FRRemunerationId, invention.UpdatedBy, PatInventorInvs, new List<PatInventorInv>() { }, new List<PatInventorInv>() { });
                    }
                    else
                    {
                        int orderOfEntry = 1;

                        PatInventorInvs.ForEach(c =>
                        {
                            c.FRRemunerationId = remuneration.FRRemunerationId;
                            //var inventor = inventors.FirstOrDefault(d => d.InventorID == c.InventorID);
                            //c.PositionId = inventor == null ? null : inventor.PositionId;
                            //var inventionPosition = inventorPositions.FirstOrDefault(d => d.PositionId == c.PositionId);
                            //c.PositionA = inventionPosition == null ? null : inventionPosition.PositionA;
                            //c.PositionB = inventionPosition == null ? null : inventionPosition.PositionB;
                            //c.PositionC = inventionPosition == null ? null : inventionPosition.PositionC;

                            if (orderOfEntry <= noOfInvetorsToAward)
                            {
                                var v = _mapper.Map<InventionInventorFRRemunerationInventorInfoViewModel>(c);
                                c.FRThirdPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "In Use Award");
                                c.FRSecondPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "First Filing Award");
                                c.FRFirstPayment = GetPaymentAmount(remuneration.FRRemunerationId, v, "Intial Award");
                            }

                            orderOfEntry++;


                        });

                        await _patInventorInvService.Update(remuneration.FRRemunerationId, invention.UpdatedBy, PatInventorInvs, new List<PatInventorInv>() { }, new List<PatInventorInv>() { });

                    }

                    //Create Init Product Sale
                    //if ((settings.InventorRemunerationPayOption.ToLower() == "both" || settings.InventorRemunerationPayOption.ToLower() == "yearly") && settings.IsInventorRemunerationUsingProductSalesOn)
                    //{
                    //    var countryApps = _applicationService.CountryApplications.Where(c => c.InvId == invention.InvId && c.IssDate != null && c.PatApplicationStatus != null && c.PatApplicationStatus.ActiveSwitch && c.Products != null && c.Products.Any()).OrderBy(c => c.IssDate);
                    //    List<PatIRFRProductSale> productSales = new List<PatIRFRProductSale>();
                    //    var appIds = countryApps.Select(c => c.AppId).ToArray();
                    //    var products = _productService.QueryableList.Where(c => _applicationService.QueryableChildList<PatProduct>().Any(a => appIds.Contains(a.AppId) && a.ProductId == c.ProductId));
                    //    var appCountries = countryApps.Select(c => c.Country).ToList();
                    //    foreach (var country in appCountries)
                    //    {
                    //        foreach (var product in products)
                    //        {
                    //            var sales = _saleService.QueryableList.Where(c => c.ProductId == product.ProductId && c.Country == country);
                    //            foreach (var sale in sales)
                    //            {
                    //                PatIRFRProductSale productSale = new PatIRFRProductSale()
                    //                {
                    //                    RemunerationId = remuneration.FRRemunerationId,
                    //                    Product = product.ProductName,
                    //                    Country = country,
                    //                    Year = sale.Yr,
                    //                    LicenseFactor = 0,
                    //                    InventionValue = 0,
                    //                    UnitPrice = 0,
                    //                    Quantity = 0,
                    //                    CurrencyType = sale.CurrencyType,
                    //                    Revenue = (double)sale.Net,
                    //                    UseOverrideRevenue = false,
                    //                };
                    //                productSales.Add(productSale);
                    //            }
                    //        }
                    //    }
                    //    await _patProductSaleService.Add(productSales);
                    //}
                }
            }
        }

        public async Task DeleteRemuneration(int invId, bool fromInvention = false)
        {

            var remuneration = _patRemunerationEntityService.QueryableList.FirstOrDefault(c => c.InvId == invId);
            if (remuneration == null)
                return;

            //delete matrix data
            await _iRValuationMatrixDataService.Delete(_iRValuationMatrixDataService.QueryableList.Where(c => c.FRRemunerationId == remuneration.FRRemunerationId));

            //delete product sales
            //await _patProductSaleService.Delete(_patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == remuneration.FRRemunerationId));

            //delete inventor distribution and update inventorInv
            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.InvId == invId).ToList();

            PatInventorInvs.ForEach(c =>
            {
                //c.Percentage = 0;
                //c.PaidByLumpSum = false;
                //c.LumpSumAmount = null;
                //c.LumpSumPaidDate = null;
                //c.RemunerationId = null;
                c.FRRemunerationId = null;
                c.RemunerationRemarks = null;
                //c.InitialPayment = null;
                //c.PositionA = null;
                //c.PositionB = null;
                //c.PositionC = null;
                //c.BuyingRightsAmount = null;
                //c.BuyingRightsDate = null;
                c.FRFirstPayment = null;
                c.FRFirstPaymentDate = null;
                c.FRSecondPayment = null;
                c.FRSecondPaymentDate = null;
                c.FRThirdPayment = null;
                c.FRThirdPaymentDate = null;
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
                invention.UseInventorFRRemuneration = false;
                await _inventionService.Update(invention);
            }
        }

        public int GetRemunerationId(int invId)
        {
            int remunerationId = 0;
            var remuneration = _patRemunerationEntityService.QueryableList.FirstOrDefault(c => c.InvId == invId);
            if (remuneration != null)
            {
                remunerationId = remuneration.FRRemunerationId;
            }
            return remunerationId;
        }

        public IQueryable<PatInventorInv> GetAvaliablePatInventorInvs(int invId)
        {
            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.InvId == invId && !c.PaidByLumpSum); // && c.InventorInvInventor.Citizenship != null && c.InventorInvInventor.Citizenship.ToUpper().Equals("DE") && !c.PaidByLumpSum);
            return PatInventorInvs;
        }

        public InventorFRRemunerationValuationMatrixViewModel GetInventorRemunerationValuationMatrixViewModels(int remunerationId)
        {
            InventorFRRemunerationValuationMatrixViewModel viewModel = new InventorFRRemunerationValuationMatrixViewModel();
            var formulaFactors = _mapper.Map<List<IRFRFormulaFactorViewModel>>(_iRFormulaFactorService.QueryableList);
            var matrixes = _mapper.Map<List<IRFRValuationMatrixViewModel>>(_iRValuationMatrixService.QueryableList.Where(c => c.ActiveSwitch));
            var criteria = _mapper.Map<List<IRFRValuationMatrixCriteriaViewModel>>(_iRValuationMatrixCriteriaService.QueryableList.Where(c => c.ActiveSwitch));

            var currentData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FRRemunerationId == remunerationId).ToList();
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

            matrixes.ForEach(c =>
            {
                c.IRFRMatrixType = _iRValuationMatrixTypeService.QueryableList.FirstOrDefault(d => d.MatrixType == c.MatrixType);
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
            });

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

        public async Task UpdateMatrixes(InventionInventorFRRemunerationViewModel matrixData, string userName)
        {
            var manualEntryOptions = PraseManualEntryOptions(matrixData);
            var initData = PraseMatrixData(matrixData, manualEntryOptions);
            var currentData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FRRemunerationId == matrixData.FRRemunerationId).ToList();
            var data = CalculateMatrixData(initData);
            var added = new List<PatIRFRRemunerationValuationMatrixData>();
            var updated = new List<PatIRFRRemunerationValuationMatrixData>();
            var deleted = new List<PatIRFRRemunerationValuationMatrixData>();
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

            await _iRValuationMatrixDataService.Update(matrixData.FRRemunerationId, userName, updated, added, deleted);
        }

        private bool NullOrEqual(int? a, int? b)
        {
            if ((a == null && b == null) || a == b)
                return true;
            return false;
        }

        private List<IRFRManualEntryOptions> PraseManualEntryOptions(InventionInventorFRRemunerationViewModel matrixData)
        {
            var result = new List<IRFRManualEntryOptions>();
            if (!String.IsNullOrEmpty(matrixData.MatrixData))
            {
                var matrixDataItem = matrixData.MatrixData.Split("|");
                foreach (var item in matrixDataItem)
                {
                    var itemList = item.Split('~');
                    if (itemList[0].Equals("FactorManualId"))
                    {
                        IRFRManualEntryOptions data = new IRFRManualEntryOptions()
                        {
                            FactorManualId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2])
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("MatrixManualId"))
                    {
                        IRFRManualEntryOptions data = new IRFRManualEntryOptions()
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

        private List<PatIRFRRemunerationValuationMatrixData> PraseMatrixData(InventionInventorFRRemunerationViewModel matrixData, List<IRFRManualEntryOptions> options)
        {
            var result = new List<PatIRFRRemunerationValuationMatrixData>();
            if (!String.IsNullOrEmpty(matrixData.MatrixData))
            {
                var matrixDataItem = matrixData.MatrixData.Split("|");
                foreach (var item in matrixDataItem)
                {
                    var itemList = item.Split('~');
                    if (itemList[0].Equals("FactorId"))
                    {
                        PatIRFRRemunerationValuationMatrixData data = new PatIRFRRemunerationValuationMatrixData()
                        {
                            FRRemunerationId = matrixData.FRRemunerationId,
                            FactorId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2]),
                            UseManualEntry = options.Any(c => c.FactorManualId == Convert.ToInt32(itemList[1]) && c.ActualValue == 1)
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("MatrixId"))
                    {
                        PatIRFRRemunerationValuationMatrixData data = new PatIRFRRemunerationValuationMatrixData()
                        {
                            FRRemunerationId = matrixData.FRRemunerationId,
                            MatrixId = Convert.ToInt32(itemList[1]),
                            ActualValue = Convert.ToDouble(itemList[2]),
                            UseManualEntry = options.Any(c => c.MatrixManualId == Convert.ToInt32(itemList[1]) && c.ActualValue == 1)
                        };
                        result.Add(data);
                    }
                    else if (itemList[0].Equals("CriteriaId"))
                    {
                        PatIRFRRemunerationValuationMatrixData data = new PatIRFRRemunerationValuationMatrixData()
                        {
                            FRRemunerationId = matrixData.FRRemunerationId,
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

        private List<PatIRFRRemunerationValuationMatrixData> CalculateMatrixData(List<PatIRFRRemunerationValuationMatrixData> initData)
        {
            var result = new List<PatIRFRRemunerationValuationMatrixData>();
            var criteria = initData.Where(c => c.CriteriaId != null).ToList();
            var matrixes = initData.Where(c => c.MatrixId != null).ToList();
            var matrixesResult = new List<PatIRFRRemunerationValuationMatrixData>();
            var factors = initData.Where(c => c.FactorId != null).ToList();
            var factorsResult = new List<PatIRFRRemunerationValuationMatrixData>();

            foreach (var matrixData in matrixes)
            {
                if (!matrixData.UseManualEntry)
                    matrixesResult.Add(CalculateMatrixValue(matrixData, criteria));
                else
                    matrixesResult.Add(matrixData);
            }

            foreach (var factorData in factors)
            {
                if (!factorData.UseManualEntry)
                    factorsResult.Add(CalculateFactorValue(factorData, matrixesResult));
                else
                    factorsResult.Add(factorData);
            }

            result.AddRange(criteria);
            result.AddRange(matrixesResult);
            result.AddRange(factorsResult);

            return result;
        }

        private PatIRFRRemunerationValuationMatrixData CalculateMatrixValue(PatIRFRRemunerationValuationMatrixData matrixViewModel, List<PatIRFRRemunerationValuationMatrixData> criteriaViewModel)
        {
            var matrix = _iRValuationMatrixService.QueryableList.FirstOrDefault(c => c.MatrixId == matrixViewModel.MatrixId);
            if (matrix == null)
                return matrixViewModel;

            var criteria = _iRValuationMatrixCriteriaService.QueryableList.Where(c => c.MatrixId == matrixViewModel.MatrixId);

            if (criteria.Count() == 0)
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

        private PatIRFRRemunerationValuationMatrixData CalculateFactorValue(PatIRFRRemunerationValuationMatrixData factorViewModel, List<PatIRFRRemunerationValuationMatrixData> matrixViewModel)
        {
            var factor = _iRFormulaFactorService.QueryableList.FirstOrDefault(c => c.FactorId == factorViewModel.FactorId);
            if (factor == null)
                return factorViewModel;

            var validMatrixViewModel = _iRValuationMatrixDataService.QueryableList.Where(c => c.MatrixId != null && c.FRRemunerationId == factorViewModel.FRRemunerationId).ToList();
            validMatrixViewModel.RemoveAll(c => matrixViewModel.Any(d => c.MatrixId == d.MatrixId));
            validMatrixViewModel.AddRange(matrixViewModel);
            var matrixes = _iRValuationMatrixService.QueryableList.ToList();

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

        public double? GetLumpSumAmount(int remunerationId, InventionInventorFRRemunerationInventorInfoViewModel invt)
        {
            double result = 0;
            var PercentageOfOwnerShip = GetTotalInventorFactor(invt);
            var PercentageOfInvention = (invt.Percentage ?? 0) / 100;
            var PatIRFRProductSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == remunerationId);
            var sales = PatIRFRProductSales.Where(c => c.Year == DateTime.Now.Year);

            var formulas = _iRRemunerationFormulaService.QueryableList.Where(c => c.RemunerationType.Equals("Lump Sum")).OrderByDescending(c => c.EffStartDate);
            var calculateDate = DateTime.Now;
            var formula = formulas.FirstOrDefault(c => (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate != null && c.EffEndDate >= calculateDate));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => ((c.EffStartDate == null) && (c.EffEndDate != null && c.EffEndDate >= calculateDate)) || (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate == null));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => c.EffStartDate == null && c.EffEndDate == null);

            var formulaText = "";
            if (formula != null)
                formulaText = formula.Formula;
            else
                return null;

            formulaText = formulaText.Replace("{%ofOwnership}", PercentageOfOwnerShip.ToString());
            formulaText = formulaText.Replace("{%ofInvention}", PercentageOfInvention.ToString());
            formulaText = formulaText.Replace("{InitialPayment}", (invt.InitialPayment ?? 0).ToString());

            var formulaFactors = _iRFormulaFactorService.QueryableList.ToList();
            var formulaFactorData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FactorId != null && c.FRRemunerationId == invt.FRRemunerationId).ToList();

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

            if (formulaText.Contains("{LicenseFactor}") || formulaText.Contains("{Revenue}"))
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
                    formulaTextCopy = formulaTextCopy.Replace("{LicenseFactor}", (sale.LicenseFactor / 100).ToString());
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

        public double? GetPaymentAmount(int remunerationId, InventionInventorFRRemunerationInventorInfoViewModel invt, string RemunerationType)
        {
            double result = 0;
            var PercentageOfOwnerShip = GetTotalInventorFactor(invt);
            var PercentageOfInvention = (invt.Percentage ?? 0) / 100;
            var PatIRFRProductSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == remunerationId);
            var sales = PatIRFRProductSales.Where(c => c.Year == DateTime.Now.Year);
            var PatInventorInvs = _patInventorInvService.QueryableList.Where(c => c.FRRemunerationId == remunerationId).ToList();

            var numOfInventors = PatInventorInvs is null ? 0 : PatInventorInvs.Count();
            var settings = _patSettings.GetSetting().Result;
            var noOfInvetorsToAward = settings.InventorFRRemunerationNoInventors;
            if (noOfInvetorsToAward == 0)
                noOfInvetorsToAward = int.MaxValue;
            numOfInventors = numOfInventors > noOfInvetorsToAward ? noOfInvetorsToAward : numOfInventors;

            var formulas = _iRRemunerationFormulaService.QueryableList.Where(c => c.RemunerationType.Equals(RemunerationType)).OrderByDescending(c => c.EffStartDate);
            var calculateDate = DateTime.Now;
            var formula = formulas.FirstOrDefault(c => (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate != null && c.EffEndDate >= calculateDate));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => ((c.EffStartDate == null) && (c.EffEndDate != null && c.EffEndDate >= calculateDate)) || (c.EffStartDate != null && c.EffStartDate <= calculateDate) && (c.EffEndDate == null));
            if (formula == null)
                formula = formulas.FirstOrDefault(c => c.EffStartDate == null && c.EffEndDate == null);

            var formulaText = "";
            if (formula != null)
                formulaText = formula.Formula;
            else
                return 0;

            formulaText = formulaText.Replace("{%ofOwnership}", PercentageOfOwnerShip.ToString());
            formulaText = formulaText.Replace("{%ofInvention}", PercentageOfInvention.ToString());
            formulaText = formulaText.Replace("{#ofInventors}", numOfInventors.ToString());
            //formulaText = formulaText.Replace("{InitialPayment}", (invt.InitialPayment ?? 0).ToString());

            var formulaFactors = _iRFormulaFactorService.QueryableList.ToList();
            var formulaFactorData = _iRValuationMatrixDataService.QueryableList.Where(c => c.FactorId != null && c.FRRemunerationId == invt.FRRemunerationId).ToList();

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

            if (formulaText.Contains("Math."))
            {
                formulaText = ReplaceMathFunction(formulaText);
            }

            //remove later
            if (formulaText.Contains("{LicenseFactor}") || formulaText.Contains("{Revenue}"))
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
                    formulaTextCopy = formulaTextCopy.Replace("{LicenseFactor}", (sale.LicenseFactor / 100).ToString());
                    formulaTextCopy = formulaTextCopy.Replace("{Revenue}", ((sale.Revenue ?? 0) / rate).ToString());
                    formulaTextCopy = ReplaceVariableToOne(formulaTextCopy);
                    DataTable dt = new DataTable();
                    var ComputeResult = dt.Compute(formulaTextCopy, "");
                    result += double.Parse(ComputeResult.ToString());
                }
            }
            else
            {
                if (formulaText.Contains("{") && formulaText.Contains("}")) return 0;
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

        private string ReplaceMathFunction(string formula)
        {
            if (formula.Contains("Math."))
            {
                DataTable tmp = new DataTable();

                if ((formula.Length - formula.Replace("Math.", "").Length) / 5 > 1) throw new Exception("Only one Math fuctnion is allowed.");

                string partial = "";
                int tmpresult = 0;
                double tmpresultDouble = 0;

                string mathEquation = "Math.Round(";
                if (formula.Contains(mathEquation))
                {
                    partial = GetMathPortion(formula, mathEquation);
                    tmpresult = (int)Math.Round(double.Parse(tmp.Compute(partial.Substring(mathEquation.Length, partial.Length - mathEquation.Length - 1), null).ToString()));
                    formula = formula.Replace(partial.ToString(), tmpresult.ToString());
                }

                mathEquation = "Math.Ceiling(";
                if (formula.Contains(mathEquation))
                {
                    partial = GetMathPortion(formula, mathEquation);
                    tmpresult = (int)Math.Ceiling(double.Parse(tmp.Compute(partial.Substring(mathEquation.Length, partial.Length - mathEquation.Length - 1), null).ToString()));
                    formula = formula.Replace(partial.ToString(), tmpresult.ToString());
                }

                mathEquation = "Math.Floor(";
                if (formula.Contains(mathEquation))
                {
                    partial = GetMathPortion(formula, mathEquation);
                    tmpresult = (int)Math.Floor(double.Parse(tmp.Compute(partial.Substring(mathEquation.Length, partial.Length - mathEquation.Length - 1), null).ToString()));
                    formula = formula.Replace(partial.ToString(), tmpresult.ToString());
                }

                mathEquation = "Math.log(";
                if (formula.Contains(mathEquation))
                {
                    partial = GetMathPortion(formula, mathEquation);
                    tmpresultDouble = (double)Math.Log(double.Parse(tmp.Compute(partial.Substring(mathEquation.Length, partial.Length - mathEquation.Length - 1), null).ToString()));
                    formula = formula.Replace(partial.ToString(), tmpresultDouble.ToString());
                }
            }

            return formula;
        }

        private string GetMathPortion(string formula, string mathEquation)
        {
            int i = formula.LastIndexOf(mathEquation);
            int firstCloseBracket = formula.IndexOf(')', formula.IndexOf(mathEquation));
            string partial = formula.Substring(i, firstCloseBracket - i + 1);
            int openBracketAmount = formula.Count(f => f == '(');
            partial = formula.Substring(i, formula.Length - i);
            int lastCloseBracket = GetNthIndex(partial, ')', openBracketAmount);
            partial = formula.Substring(i, lastCloseBracket - i + 1);
            return partial;
        }

        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            throw new Exception("Please check parentheses");
            return -1;
        }

        public InventionInventorFRRemunerationProductSaleStageInfoViewModel GetStaggeredInfo(int productSaleId)
        {
            InventionInventorFRRemunerationProductSaleStageInfoViewModel model = new InventionInventorFRRemunerationProductSaleStageInfoViewModel();
            var productSale = _patProductSaleService.GetByIdAsync(productSaleId).Result;
            var productSales = _patProductSaleService.QueryableList.Where(c => c.FRRemunerationId == productSale.FRRemunerationId);

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

                model.PrevRevenuesAccumulatedEuro += ((sale.Revenue ?? 0) * (sale.InventionValue ?? 0)) / rate / 100;
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

                model.CurrentRevenuesAccumulatedEuro += ((sale.Revenue ?? 0) * (sale.InventionValue ?? 0)) / rate / 100;
            }

            var erDM = _patEuroExchangeRateService.QueryableList.FirstOrDefault(c => c.CurrencyType.ToLower() == "DM".ToLower());
            if (erDM == null)
                throw new NullReferenceException("DM " + _localizer["exchange rate is not found."]);
            if (erDM.UseDefault)
            {
                model.ExchangeRate = erDM.DefaultExchangeRate;
            }
            else
            {
                var ery = _patEuroExchangeRateYearlyService.QueryableList.FirstOrDefault(c => c.ExchangeId == erDM.ExchangeId && c.Year == productSale.Year);
                if (ery == null)
                    throw new NullReferenceException("DM " + _localizer["exchange rate is not found for the year"] + " " + productSale.Year + ". " + _localizer["Please add the exchange rate first."]);
                model.ExchangeRate = ery.ExchangeRate;
            }
            model.PrevRevenuesAccumulatedDM = model.PrevRevenuesAccumulatedEuro * model.ExchangeRate;
            model.CurrentRevenuesAccumulatedDM = model.CurrentRevenuesAccumulatedEuro * model.ExchangeRate;

            PatIRFRStaggering staggering = _patStaggeringEntityService.QueryableList.Where(c => c.Year <= productSale.Year).OrderByDescending(c => c.Year).First();
            IQueryable<PatIRFRStaggeringDetail> staggeringDetails = _patStaggeringDetailService.QueryableList.Where(c => c.StaggeringId == staggering.StaggeringId).OrderBy(c => c.AmountFrom);

            IQueryable<PatIRFRStaggeringDetail> previousStaggeringDetails = staggeringDetails.Where(c => c.AmountFrom == null || c.AmountFrom < model.PrevRevenuesAccumulatedDM).OrderBy(c => c.AmountFrom);
            IQueryable<PatIRFRStaggeringDetail> currentStaggeringDetails = staggeringDetails.Where(c => c.AmountFrom == null || c.AmountFrom < model.CurrentRevenuesAccumulatedDM).OrderBy(c => c.AmountFrom);

            List<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel> previousStages = new List<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel>();
            List<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel> currentStages = new List<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel>();

            foreach (var d in previousStaggeringDetails)
            {
                InventionInventorFRRemunerationProductSaleStageInfoStageViewModel stage = new InventionInventorFRRemunerationProductSaleStageInfoStageViewModel();
                stage.Stage = d.Stage;
                stage.Amount = (double)((d.AmountTo == null || d.AmountTo >= model.PrevRevenuesAccumulatedDM)
                    ? model.PrevRevenuesAccumulatedDM - (d.AmountFrom ?? 0)
                    : (d.AmountTo ?? 0) - (d.AmountFrom ?? 0)
                    ) * (1 - d.Reduction);
                previousStages.Add(stage);
            }
            model.PrevStages = previousStages.AsQueryable();

            foreach (var d in currentStaggeringDetails)
            {
                InventionInventorFRRemunerationProductSaleStageInfoStageViewModel stage = new InventionInventorFRRemunerationProductSaleStageInfoStageViewModel();
                stage.Stage = d.Stage;
                stage.Amount = (double)((d.AmountTo == null || d.AmountTo >= model.CurrentRevenuesAccumulatedDM)
                    ? model.CurrentRevenuesAccumulatedDM - (d.AmountFrom ?? 0)
                    : (d.AmountTo ?? 0) - (d.AmountFrom ?? 0)
                    ) * (1 - d.Reduction);
                currentStages.Add(stage);
            }
            model.CurrentStages = currentStages.AsQueryable();

            model.PrevRevenuesStaggeredDM = model.PrevStages.Sum(c => c.Amount);
            model.CurrentRevenuesStaggeredDM = model.CurrentStages.Sum(c => c.Amount);
            model.RevenueForRemunerationDM = model.CurrentRevenuesStaggeredDM - model.PrevRevenuesStaggeredDM;
            model.RevenueForRemunerationEuro = model.RevenueForRemunerationDM / model.ExchangeRate;

            return model;
        }

        public async Task ProcessImportedProductSales(string userName)
        {
            var added = _saleService.QueryableList.Where(c => (c.RecentAddedByImport ?? false)).ToList();
            var updated = _saleService.QueryableList.Where(c => (c.RecentUpdatedByImport ?? false)).ToList();

            var settings = await _patSettings.GetSetting();
            var addedForProductSales = added.Where(c => c.Yr >= settings.ImportInventorRemunerationProductSalesCutOffYear).OrderByDescending(c => c.Yr).ToList();
            var updatedForProductSales = updated.Where(c => c.Yr >= settings.ImportInventorRemunerationProductSalesCutOffYear).OrderByDescending(c => c.Yr).ToList();

            try
            {
                if (addedForProductSales.Count() > 0)
                    await SharedProductSalesAdd(addedForProductSales, userName);
                if (updatedForProductSales.Count() > 0)
                    await SharedProductSalesUpdate(updatedForProductSales, userName);
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
            var product = _productService.QueryableList.First(c => c.ProductId == deleted.ProductId);
            var IRFRSales = _patProductSaleService.QueryableList.Where(c => c.Year == deleted.Yr &&
                (c.Country ?? "") == (deleted.Country ?? "") &&
                c.Product == product.ProductName &&
                ((c.CurrencyType ?? "") == (deleted.CurrencyType ?? ""))).ToList();
            await _patProductSaleService.Delete(IRFRSales);
            await UpdateRemunerationUpdatedYears(IRFRSales);

            if (deleted.Net != 0 || string.IsNullOrEmpty(deleted.CurrencyType))
            {
                foreach (var IRFRsale in IRFRSales)
                {
                    try
                    {
                        await DeleteDistribution(IRFRsale);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
        public async Task SharedProductSalesUpdate(List<ProductSale> updated, string userName)
        {
            var updatedSales = new List<PatIRFRProductSale>();
            foreach (var update in updated)
            {
                var product = _productService.QueryableList.First(c => c.ProductId == update.ProductId);
                var IRFRSales = _patProductSaleService.QueryableList.Where(c => c.Year == (update.ProductSaleBeforeUpdate ?? update).Yr &&
                    (c.Country ?? "") == ((update.ProductSaleBeforeUpdate ?? update).Country ?? "") &&
                    c.Product == product.ProductName &&
                    (c.CurrencyType ?? "") == (update.ProductSaleBeforeUpdate != null ? update.ProductSaleBeforeUpdate.CurrencyType : update.CurrencyTypeBeforeImport)).ToList();
                IRFRSales.ForEach(c =>
                {
                    c.Year = update.Yr;
                    c.Country = update.Country;
                    c.CurrencyType = update.CurrencyType;
                    c.Revenue = c.UseOverrideRevenue ? c.Revenue : (double)update.Net;
                });
                updatedSales.AddRange(IRFRSales);
            }

            await _patProductSaleService.Update(updatedSales);
            var remunerationIds = updatedSales.Select(c => c.FRRemunerationId).Distinct().ToList();

            await UpdateRemunerationUpdatedYears(updatedSales);

            foreach (var remunerationId in remunerationIds)
            {
                try
                {
                    await UpdateDistribution(remunerationId, new List<PatIRFRProductSale>(), updatedSales.Where(c => c.FRRemunerationId == remunerationId).ToList());
                }
                catch (Exception e)
                {

                }
            }
        }
        public async Task SharedProductSalesAdd(List<ProductSale> added, string userName)
        {
            var addedSales = new List<PatIRFRProductSale>();
            var remunerations = _patRemunerationEntityService.QueryableList.ToList();
            foreach (var add in added)
            {
                var product = _productService.QueryableList.First(c => c.ProductId == add.ProductId);
                var inventions = _inventionService.QueryableList.Where(c => c.UseInventorFRRemuneration && _patRemunerationEntityService.QueryableList.Any(d => d.InvId == c.InvId));
                foreach (var invention in inventions)
                {
                    var remuneration = remunerations.FirstOrDefault(c => c.InvId == invention.InvId);
                    if (remuneration == null) continue;

                    var havingApplications = _applicationService.CountryApplications.Any(c => c.InvId == invention.InvId && c.Country == add.Country && c.IssDate != null && c.PatApplicationStatus != null && c.PatApplicationStatus.ActiveSwitch && c.Products != null && c.Products.Any(d => d.ProductId == add.ProductId));

                    if (!_patProductSaleService.QueryableList.Any(c => c.FRRemunerationId == remuneration.FRRemunerationId &&
                    c.Year == add.Yr &&
                    (c.Country ?? "") == (add.Country ?? "") &&
                    c.Product == product.ProductName &&
                    (c.CurrencyType ?? "") == (add.CurrencyType ?? "")))
                    {
                        PatIRFRProductSale productSale = new PatIRFRProductSale()
                        {
                            FRRemunerationId = remuneration.FRRemunerationId,
                            Product = product.ProductName,
                            Country = add.Country,
                            Year = add.Yr,
                            LicenseFactor = 0,
                            InventionValue = 0,
                            UnitPrice = 0,
                            Quantity = 0,
                            CurrencyType = add.CurrencyType,
                            Revenue = (double)add.Net,
                            UseOverrideRevenue = false,
                        };
                        addedSales.Add(productSale);
                    }
                }
            }
            await _patProductSaleService.Add(addedSales);
            await UpdateRemunerationUpdatedYears(addedSales);
        }

        private async Task UpdateRemunerationUpdatedYears(List<PatIRFRProductSale> patIRFRProductSales)
        {
            var remunerationIds = patIRFRProductSales.Select(c => c.FRRemunerationId).Distinct().ToList();
            var remunerations = _patRemunerationEntityService.QueryableList.ToList();
            var updatedRemunerations = new List<PatIRFRRemuneration>();
            foreach (var remunerationId in remunerationIds)
            {
                var remuneration = remunerations.First(c => c.FRRemunerationId == remunerationId);
                var currentProductSalesUpdatedYears = (remuneration.ProductSalesUpdatedYears == null ? "" : remuneration.ProductSalesUpdatedYears);
                var currentYears = currentProductSalesUpdatedYears == "" ? new List<int>() : (currentProductSalesUpdatedYears.Contains(", ") ? currentProductSalesUpdatedYears.Split(", ").Select(Int32.Parse).ToList() : new List<int>() { int.Parse(currentProductSalesUpdatedYears) });
                var currentSaleYears = patIRFRProductSales.Where(c => c.FRRemunerationId == remunerationId).Select(c => c.Year).Cast<int>().ToList();
                currentYears.AddRange(currentSaleYears);
                currentYears = currentYears.Distinct().ToList();
                currentYears.Sort();
                var updatedProductSalesUpdatedYears = string.Join(", ", currentYears.ToArray());
                remuneration.ProductSalesUpdatedYears = updatedProductSalesUpdatedYears;
                updatedRemunerations.Add(remuneration);
            }
            await _patRemunerationEntityService.Update(updatedRemunerations);
        }
    }
}
