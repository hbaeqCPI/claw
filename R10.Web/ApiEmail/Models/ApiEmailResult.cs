using R10.Core.DTOs;

namespace R10.Web.ApiEmail.Models
{
    public class ApiEmailResult 
    {
        public KeyTextDTO[] Results { get; set; }

        public int PageCount { get; set; }
        public int RowCount { get; set; }

        public ApiEmailResult()
        {
            Results = new KeyTextDTO[] { };
        }
    }
}
