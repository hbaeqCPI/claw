using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiCompression
    {
        byte[] Compress(MemoryStream stream);
        MemoryStream Decompress(MemoryStream stream);
        DataSet DecompressToDataSet(byte[] data);
    }
}
