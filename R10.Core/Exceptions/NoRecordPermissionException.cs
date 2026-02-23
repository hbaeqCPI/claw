using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Exceptions
{

    public class NoRecordPermissionException : Exception
    {
        public NoRecordPermissionException():base("Access to the record is denied or record was deleted.") 
        {
        }

        protected NoRecordPermissionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public NoRecordPermissionException(string message) : base(message)
        {
        }

        public NoRecordPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
