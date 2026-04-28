using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace LawPortal.Core.Services
{
    /// <summary>
    /// Ported from R8\CPi.Web.RW\App_Codes\Globals\CPiUtilities.vb
    /// Public Class clsCompression
    /// Used when calling CPiEARSCommunication
    /// Must match Decompress method in CPiEARSCommunication
    /// </summary>
    public class CPiCompression : ICPiCompression
    {
        public byte[] Compress(MemoryStream msIn)
        {
            byte[] compressed = { };

            if (msIn.Length > 0)
            {
                using (var msOut = new MemoryStream())
                {
                    using (var gs = new GZipStream(msOut, CompressionMode.Compress))
                    {
                        var data = msIn.ToArray();
                        gs.Write(data, 0, data.Length);
                    }
                    compressed = msOut.ToArray();
                }
            }

            return compressed;
        }

        public MemoryStream Decompress(MemoryStream msIn)
        {
            var msOut = new MemoryStream();

            if (msIn.Length > 0)
            {
                msIn.Position = 0;
                using (var gs = new GZipStream(msIn, CompressionMode.Decompress))
                {
                    //ported from R8
                    //not sure how this works
                    //look like an infinite loop
                    //var size = 512;
                    //var data = new byte[size];

                    //while (size != 0)
                    //{
                    //    size = gs.Read(data, 0, 512);
                    //    msOut.Write(data, 0, size);
                    //}

                    gs.CopyTo(msOut);
                }
            }

            return msOut;
        }

        public DataSet DecompressToDataSet(byte[] data)
        {
            var ds = new DataSet();

            if (data.Length > 0)
            {
                using (var msOut = Decompress(new MemoryStream(data, false)))
                {
                    if (msOut.Length > 0)
                    {
                        msOut.Capacity = (int)msOut.Length;
                        msOut.Position = 0;

                        ds.ReadXml(msOut, XmlReadMode.Auto);
                    }
                }
            }

            return ds;
        }
    }
}
