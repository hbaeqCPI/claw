using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiEncryption
    {
        string Encrypt(string text, string key = "", bool caseSensitive = false);
        string Decrypt(string encrypted, bool withTimeKey = false);
    }
}
