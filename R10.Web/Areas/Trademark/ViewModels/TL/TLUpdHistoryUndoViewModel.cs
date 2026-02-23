
namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TLUpdHistoryUndoViewModel
    {
        public int LogId { get; set; }
        public int TmkId { get; set; }
        public int RevertType { get; set; }
        public int JobId { get; set; }
        public string? UpdateType { get; set; }
    }
}
