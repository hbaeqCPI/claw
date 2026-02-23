using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Exceptions
{

    public class ValueNotAllowedException : Exception
    {
        public ValueNotAllowedException()
        {
        }
        protected ValueNotAllowedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
        public ValueNotAllowedException(string message) : base(message)
        {
        }
        public ValueNotAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
