using R10.Core.DTOs;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickDocketDefaultSettingsViewModel : QuickDocketDefaultSettingsDTO
    {        
        public string? Patent { get; set; }
        public string? PTOActions { get; set; }
        public string? Trademark { get; set; }
        public string? TrademarkLinks { get; set; }
        public string? GeneralMatter { get; set; }
        public string? DMS { get; set; }
        public string? AMS { get; set; }
        public string? AttorneyFilter1 { get; set; }
        public string? AttorneyFilter2 { get; set; }
        public string? AttorneyFilter3 { get; set; }
        public string? AttorneyFilter4 { get; set; }
        public string? AttorneyFilter5 { get; set; }
        public string? AttorneyFilterR { get; set; }
        public string? AttorneyFilterD { get; set; }

        public List<QuickDocketSystemTypeViewModel>? Systems { get; set; }
    }

}
