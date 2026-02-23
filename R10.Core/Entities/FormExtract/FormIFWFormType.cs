using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities.FormExtract
{
    public class FormIFWFormType
    {
        [Key]
        public int FormTypeId { get; set; }

        public string FormType { get; set; }
        public string FormName { get; set; }
        public string DetailView { get; set; }
        public bool IsEnabled { get; set; }

        public List<FormIFWDocType>? FormIFWDocTypes { get; set; }
    }
}
