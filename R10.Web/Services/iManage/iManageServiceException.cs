using System.Net;
using System.Runtime.Serialization;

namespace R10.Web.Services.iManage
{
    public class iManageServiceException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }

        public iManageServiceException(string? message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public iManageServiceException(string? message, Exception? innerException, HttpStatusCode statusCode) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        protected iManageServiceException(SerializationInfo info, StreamingContext context, HttpStatusCode statusCode) : base(info, context)
        {
            StatusCode = statusCode;
        }
    }
}
