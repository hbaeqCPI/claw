using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Security;
using R10.Web.Areas;

namespace R10.Web.Controllers
{
    [Authorize(Policy = SharedAuthorizationPolicy.DecisionMaker)]
    public class IPDecisionCentralController : BaseController
    {
        [HttpGet]
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");

        [HttpGet]
        public IActionResult DMS() => RedirectToAction("Index");
        [HttpGet]
        public IActionResult ForeignFiling(int remId) => RedirectToAction("Index");
        [HttpGet]
        public IActionResult AMS() => RedirectToAction("Index");
        [HttpGet]
        public IActionResult RMS(int remId) => RedirectToAction("Index");
        [HttpGet]
        public IActionResult PatClearance() => RedirectToAction("Index");
        [HttpGet]
        public IActionResult SearchRequest() => RedirectToAction("Index");
        [HttpGet]
        public IActionResult CostEstimator() => RedirectToAction("Index");
        [HttpGet]
        public IActionResult TmkCostEstimator() => RedirectToAction("Index");
    }

    public static class IpdcNavPages
    {
        public static string Home => "ipdcHomePage";
        public static string DisclosureReview => "dmsDisclosureReviewResults";
        public static string ForeignFilingPortfolioReview => "foreignFilingPortfolioReview";
        public static string AnnuityPortfolioReview => "amsPortfolioReview";
        public static string TrademarkRenewalPortfolioReview => "rmsPortfolioReview";
        public static string PatentClearanceReview => "patClearanceReview";
        public static string TrademarkSearchRequestReview => "tmkSearchRequestReview";
        public static string CostEstimator => "costEstimator";
        public static string TrademarkCostEstimator => "tmkCostEstimator";
    }
}
