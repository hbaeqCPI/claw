using System.ComponentModel.DataAnnotations;

namespace R10.Web.Api.Models
{
    public class AttorneyData
    {
        public int AttorneyID { get; set; }
        public string? AttorneyCode { get; set; }
        public string? AttorneyName { get; set; }
        public string? EMail { get; set; }
    }
}
