using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.FormExtract
{
    public class FormSource
    {
        [Key]
        public int SystemId { get; set; }

        public string SystemType { get; set; }
        public string SourceCode { get; set; }
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public string SearchTabView { get; set; }
        public string MainView { get; set; }

        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }
    }
}
