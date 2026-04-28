using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Services
{
    /// <summary>
    /// Ported from R8\CPiClass\Security.vb
    /// Used when calling CPiEARSCommunication
    /// </summary>
    public class CPiEncryption : ICPiEncryption
    {
        protected string EncryptionMap => "62840 19375#%^$&*(=-:];|)\\\"/?}<,>.`+[{!PGRDT_FQHVJ'ALCSE@UIWXNZ~KBMYOpgrdtfqhvjalcseuiwxnzkbmyo";

        public string Encrypt(string text, string key = "", bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var encrypted = caseSensitive ? text : text.ToUpper();

            if (!string.IsNullOrEmpty(key))
            {
                var temp = "";
                var encryptionKey = string.Concat(key.Substring(4), key.Substring(4));
                encrypted = string.Concat(key.Substring(0, 4), encrypted);

                for (int i = 0; i < encrypted.Length; i++)
                {
                    temp = string.Concat(temp, encryptionKey.Substring(i % 8, 1), encrypted.Substring(i, 1));
                }
                encrypted = temp;
            }

            encrypted = string.Concat("0-", Bin2ByteString(String2Bin(encrypted), false, true));

            return encrypted;
        }

        public string Decrypt(string encrypted, bool withTimeKey = false)
        {
            if (string.IsNullOrEmpty(encrypted))
                return encrypted;

            if ((int)encrypted[0] == 0)
                encrypted = Bin2ByteString(String2Bin(encrypted.Substring(1), true), true);
            else if (encrypted.StartsWith("0-"))
                encrypted = Bin2ByteString(String2Bin2(encrypted.Substring(2), true), true);
            else
                withTimeKey = false;

            if (withTimeKey)
            {
                var temp = "";
                for (int i = 0; i < encrypted.Length; i++)
                {
                    if (i % 2 != 0)
                        temp = string.Concat(temp, encrypted.Substring(i, 1));
                }
                encrypted = temp.Substring(4);
            }

            return encrypted;
        }

        protected string String2Bin(string text, bool isToCPi = false)
        {
            var retVal = "";

            if (isToCPi)
            {
                var template = text.Trim().Split("-");
                for (int i = 0; i < text.Length; i++)
                {
                    retVal = string.Concat(retVal, Char2Bin(int.Parse(template[i]), 8));
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    retVal = string.Concat(retVal, Char2Bin(EncryptionMap.IndexOf(text[i]) + 1, 7));
                }
            }

            return retVal;
        }

        protected string String2Bin2(string text, bool isToCPi = false)
        {
            var retVal = "";

            if (isToCPi)
            {
                var template = text.Trim().Split("-");
                for (int i = 0; i < template.Length; i++)
                {
                    retVal = string.Concat(retVal, Char2Bin(int.Parse(template[i]), 8));
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    retVal = string.Concat(retVal, Char2Bin(EncryptionMap.IndexOf(text[i]) + 1, 7));
                }
            }

            return retVal;
        }

        protected string Char2Bin(int ascii, int bits)
        {
            var bitAsInt = ascii;
            var bitAsString = "";

            while (bitAsInt > 0)
            {
                bitAsString = string.Concat(bitAsInt % 2, bitAsString);
                bitAsInt = bitAsInt / 2;
            }

            return bitAsString.PadLeft(bits).Replace(" ", "0");
        }

        protected string Bin2ByteString(string binaryString, bool isToCPi = false, bool encode = false)
        {
            var retVal = "";
            var length = binaryString.Length;

            if (isToCPi)
            {
                while (length > 0)
                {
                    length -= 7;
                    if (length < 0)
                        retVal = string.Concat(CPiChar(Bin2Byte(binaryString.Substring(0, length + 7))), retVal);
                    else
                        retVal = string.Concat(CPiChar(Bin2Byte(binaryString.Substring(length, 7))), retVal);
                }
            }
            else if (encode)
            {
                while (length > 0)
                {
                    length -= 8;
                    if (length < 0)
                        retVal = string.Concat(Bin2Byte(binaryString.Substring(0, length + 8)), "-", retVal);
                    else
                        retVal = string.Concat(Bin2Byte(binaryString.Substring(length, 8)), "-", retVal);
                }
                retVal = retVal.Substring(0, retVal.Length - 1);
            }
            else
            {
                while (length > 0)
                {
                    length -= 8;
                    if (length < 0)
                        retVal = string.Concat((char)(Bin2Byte(binaryString.Substring(0, length + 8))), retVal);
                    else
                        retVal = string.Concat((char)(Bin2Byte(binaryString.Substring(length, 8))), retVal);
                }
            }

            return retVal;
        }

        protected char CPiChar(int mapPos)
        {
            if (mapPos > 0)
                return EncryptionMap[mapPos - 1];

            return '\0';
        }

        protected int Bin2Byte(string binaryString)
        {
            var retVal = 0;

            for (int i = 0; i < binaryString.Length; i++)
            {
                retVal = (int)((retVal * 2) + Char.GetNumericValue(binaryString[i]));
            }

            return retVal;
        }
    }
}
