using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Helpers
{
    /// <summary>
    /// Value converter for column encryption
    /// Only used for string data type
    /// </summary>
    public class EncryptionConverter : ValueConverter<string?, string?> 
    {        
        public EncryptionConverter(ConverterMappingHints? mappingHints = null)
            : base(
                  v => EncryptionHelper.Encrypt(v), 
                  v => EncryptionHelper.Decrypt(v), mappingHints)
        { 
        }
    }
}
