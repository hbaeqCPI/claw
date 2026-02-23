
namespace R10.Web.Areas.Shared.ViewModels
{
    public class EFSGenerationParamViewModel
    {
        public int RecId { get; set; }
        public string? DocPath { get; set; }
        public string? DocType { get; set; }
        public string? MapFile { get; set; }
        public int PageCount { get; set; }
        public int PageNo { get; set; }
        public string? SourceTables { get; set; }
        public string? SubType { get; set; }
        public string? Signatory { get; set; }
        public int EfsDocId { get; set; }
        public string? SystemName { get; set; }
        public string? SystemType { get; set; }
        public string? DataKey { get; set; }
        public bool Preview { get; set; }
        public string? SharePointRecKey { get; set; }

    }
}
