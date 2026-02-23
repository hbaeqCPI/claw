using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace R10.Core.Exceptions
{
    public class WebApiValidationException : Exception
    {
        protected const string _message = "One or more validation errors occurred.";

        public WebApiValidationException() : base(_message)
        {
        }

        protected WebApiValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public WebApiValidationException(string message) : base(message)
        {
        }

        public WebApiValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebApiValidationException(string message, List<string> errors) : base(message, new Exception(JsonSerializer.Serialize(errors)))
        {
        }

        public WebApiValidationException(List<string> errors) : base(_message, new Exception(JsonSerializer.Serialize(errors)))
        {
        }
    }
}
