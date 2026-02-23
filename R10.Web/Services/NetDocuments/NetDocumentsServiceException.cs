using System.Net;
using System.Runtime.Serialization;

namespace R10.Web.Services.NetDocuments
{
    public class NetDocumentsServiceException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }

        public NetDocumentsServiceException(string? message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public NetDocumentsServiceException(string? message, Exception? innerException, HttpStatusCode statusCode) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        protected NetDocumentsServiceException(SerializationInfo info, StreamingContext context, HttpStatusCode statusCode) : base(info, context)
        {
            StatusCode = statusCode;
        }
    }
}
