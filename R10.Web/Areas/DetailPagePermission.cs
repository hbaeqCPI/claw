using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web
{
    public class DetailPagePermission
    {
        public DetailPagePermission()
        {
            this.PageActions = new List<DetailPageAction>();
        }
        public bool CanAddRecord { get; set; }
        public bool CanEditRecord { get; set; }
        public bool CanDeleteRecord { get; set; }
        public bool CanCopyRecord { get; set; }
        public bool CanPrintRecord { get; set; } = true;
        public bool CanEditRemarksOnly { get; set; }
        public bool HasLimitedRead { get; set; }
        public bool CanEmail { get; set; }
        public bool CanSearch { get; set; } = true;
        public bool CanViewMap { get; set; }
        public bool CanRefreshRecord { get; set; } = true;
        public bool CanGenerateLetter { get; set; } = false;
        public bool CanUploadDocuments { get; set; }

        public string FullModifyPolicy { get; set; }
        public string RemarksOnlyModifyPolicy { get; set; }
        public string DeletePolicy { get; set; }
        public string CopyPolicy { get; set; }
        public string LimitedReadPolicy { get; set; }
        public string CanUploadDocumentsPolicy { get; set; }

        public string SearchScreenUrl { get; set; }
        public string AddScreenUrl { get; set; }
        public string EditScreenUrl { get; set; }
        public string DeleteScreenUrl { get; set; }
        public string CopyScreenUrl { get; set; }
        public string PrintScreenUrl { get; set; }
        public string EmailScreenUrl { get; set; }
        public string RefreshRecordUrl { get; set; }
        public string SubmitScreenUrl { get; set; }
        public string MapScreenUrl { get; set; }
        public string LetterScreenUrl { get; set; }

        public bool ShowRecordNavigator { get; set; }
        public bool IsCopyScreenPopup { get; set; } = true;
        public string Container { get; set; }

        public List<DetailPageAction> PageActions { get; set; }

        public string DeleteConfirmationUrl { get; set; }

        public bool IsTradeSecret { get; set; }
        public string? TradeSecretLocator { get; set; }
        public TradeSecretRequest? TradeSecretUserRequest { get; set; }
        public bool ShowTradeSecretSwitch { get; set; }
        public bool ShowTradeSecretRequest { get; set; }
        public bool CanEditTradeSecret { get; set; }    //edit trade secret fields
        public bool CanDeleteTradeSecret { get; set; }  //delete record
    }

    public class DetailPageAction {
        public string Url { get; set; }
        public string Label { get; set; }
        public bool IsPopup { get; set; }
        public string IconClass { get; set; }
        public string ControlId { get; set; }
        public bool AppendContent { get; set; }
        public bool IsPageNav { get; set; } //adds page-nav class to use default click event handler
        public string Message { get; set; }
        public string? Class { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}
