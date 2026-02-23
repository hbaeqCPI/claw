namespace R10.Web.Api.Models
{
    public class ApiError
    {
        public ApiError(string? message)
        {
            Message = message;
            Errors = new List<string>();
        }

        public ApiError(string? message, List<string>? errors)
        {
            Message = message;
            Errors = errors;
        }

        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
