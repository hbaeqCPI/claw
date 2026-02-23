using R10.Core.Helpers;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    // request log
    public class TradeSecretRequest
    {
        [Key]
        public int RequestId { get; set; }

        // discriminator
        // use values from TradeSecretScreen
        [Required]
        [StringLength(50)]
        public string? ScreenId { get; set; }

        [Required]
        public int RecId { get; set; }

        [Required]
        [StringLength(450)]
        public string? UserId { get; set; }

        [Encrypted]
        [StringLength(150)]
        public string? Token { get; set; }

        // use values from TradeSecretRequestStatus
        [Required]
        [StringLength(25)]
        public string? Status { get; set; }

        [Required]
        public DateTime? RequestDate { get; set; }

        [Required]
        public DateTime? StatusDate { get; set; }
        
        [Encrypted]
        [StringLength(150)]
        public string? TimeStamp { get; set; }

        public string? Approver { get; set; }

        public int? ValidationFailedCount { get; set; } = 0;


        public bool IsExpired => Status == TradeSecretRequestStatus.Denied || Status == TradeSecretRequestStatus.Revoked ||
                                    (ValidationFailedCount ?? 0) >= TradeSecretHelper.MaxValidationFailedCount ||
                                    string.IsNullOrEmpty(TimeStamp) || 
                                    !DateTime.TryParseExact(TimeStamp, TradeSecretHelper.TimeStampFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt) || DateTime.ParseExact(TimeStamp, TradeSecretHelper.TimeStampFormat, CultureInfo.CurrentCulture).AddMinutes(TradeSecretHelper.RequestExpiration) < DateTime.Now;

        public bool IsCleared => !IsExpired && Status == TradeSecretRequestStatus.Cleared;

        public bool IsGranted => !IsExpired && Status == TradeSecretRequestStatus.Granted;

        public CPiUser? CPiUser { get; set; }
    }

    // screen name constants
    public static class TradeSecretScreen
    {
        // request sources
        public static string Invention => "Invention";
        public static string CountryApplication => "CountryApplication";

        // view log sources
        public static string Abstract => "Abstract";
        public static string InventionSearch => "InventionSearch";
        public static string CountryApplicationSearch => "CountryApplicationSearch";
        public static string ForeignFilingPortfolio => "ForeignFilingPortfolio";
        public static string ForeignFilingInstructions => "ForeignFilingInstructions";
        public static string AMSMain => "AMSMain";
        public static string AMSDue => "AMSDue";
        public static string AMSPortfolio => "AMSPortfolio";
        public static string AMSInstructions => "AMSInstructions";
        public static string DMSDisclosure => "DMSDisclosure";
        public static string DMSDisclosureSearch => "DMSDisclosureSearch";
        public static string QuickDocket => "QuickDocket";
        public static string DMSReview => "DMSReview";
        public static string DMSPreview => "DMSPreview";
        public static string Report => "Report";

        // docs download log screendId
        public static string DocFile => "DocFile";

        // docs download log sources
        public static string InventionDocuments => "InventionDocuments";
        public static string CountryApplicationDocuments => "CountryApplicationDocuments";
        public static string DisclosureDocuments => "DisclosureDocuments";
    }

    // status code constants
    public static class TradeSecretRequestStatus
    {
        public static string Pending => "Pending";  //new request
        public static string Denied => "Denied";    //request is denied
        public static string Granted => "Granted";  //request is granted. key is sent to user
        public static string Cleared => "Cleared";  //key is validated. request is cleared
        public static string Revoked => "Revoked";  //max validation failed count reached. request is revoked

        public static List<string> ApprovalList => new List<string>() { Pending, Granted, Denied };
    }
}
