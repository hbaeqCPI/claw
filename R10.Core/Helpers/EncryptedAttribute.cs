using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Helpers
{
    /// <summary>
    /// Attribute to enable column encryption
    /// Only encrypts string data type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EncryptedAttribute : Attribute
    {
    }
}
