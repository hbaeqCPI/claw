
namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSUpdHistoryUndoViewModel
    {
        public int LogId { get; set; }
        public int AppId { get; set; }
        public int RevertType { get; set; }
        public int JobId { get; set; }
        public string? UpdateType { get; set; }
    }
}
