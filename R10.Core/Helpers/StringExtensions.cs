using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Security;

namespace R10.Core.Helpers
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitIntoParts(this string value, int length)
        {
            for (int startPos = 0; startPos <= value.Length - length; startPos++)
            {
                yield return value.Substring(startPos, length);
            }
            yield break;
        }

        public static string Left(this string value, int length)
        {
            length = Math.Abs(length);

            if (string.IsNullOrEmpty(value)) return value;

            if (length >= value.Length)
                return value;

            return value.Substring(0, length);
        }

        public static string Right(this string value, int length)
        {
            length = Math.Abs(length);

            if (string.IsNullOrEmpty(value)) return value;

            if (length >= value.Length)
                return value;

            return value.Substring(value.Length - length, length);
        }

        public static  bool IsNumeric(this string value)
        {
            return long.TryParse(value, out var n);
        }

        public static bool IsAlpha(this string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsLetter(value[i]))
                    return false;
            }
            return true;
        }

        public static long? GetConsecutiveDigits(this string value)
        {
            var result =  Regex.Match(value, @"\d+").Value;

            if (!string.IsNullOrEmpty(result))
               return Convert.ToInt64(result);
            else
            {
                return null;
            }

        }

        public static int CountAllChars(this string template, string charToCount)
        {
            var reducedTemplate = template;

            if (template.IndexOf('"') > -1)
            {
                MatchCollection matches = Regex.Matches(reducedTemplate, "\".*?\"");

                foreach (Match m in matches)
                {
                    reducedTemplate = reducedTemplate.Replace(m.Value, "");
                }
            }
            return Regex.Replace(reducedTemplate, $"(?i)[^{charToCount}]+", "").Length;
        }

        public static string Compress(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string Decompress(this string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }

        public static string Encrypt(this string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string Decrypt(this string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public static (string FirstName, string LastName) SplitName(this string name)
        {
            var nameParts = name.Split(" ");
            if (nameParts.Length > 1)
                return (nameParts[0], nameParts[nameParts.Length - 1]);

            return (name, "");
        }
        public static SecureString ToSecureString(this string plainString)
        {
            if (plainString == null)
                return null;

            SecureString secureString = new SecureString();
            foreach (char c in plainString)
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }

        public static string ReplaceInvalidFilenameChars(this string fileName, char? newChar = null)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), newChar == null ? "" : newChar.ToString());
            }

            return fileName;
        }
    }
}
