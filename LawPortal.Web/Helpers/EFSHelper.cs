using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Xfa;
using iText.Kernel.Pdf;
using LawPortal.Web.Interfaces;

namespace LawPortal.Web.Helpers
{
    public class EFSHelper : IEFSHelper
    {
        public DataSet DataSource { get; set; }
        public string SourceDocumentPath { get; set; }
        public string MapFilePath { get; set; }

        public static readonly string MapFolder = @"Resources\EFS";

        public byte[] FillPdfWithData(bool isManualMerge, string docType, string subType, ref bool compressed)
        {
            if (isManualMerge)
            {
                if (docType == "PatEFS" && subType == "Decl")
                {
                    byte[] compressedFiles = null;

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            var allSource = DataSource.Tables[0].Copy();

                            foreach (DataRow row in allSource.Rows)
                            {
                                var dtSource = DataSource.Tables[0].Clone();
                                dtSource.Rows.Add(row.ItemArray);
                                DataSource.Tables.RemoveAt(0);
                                DataSource.Tables.Add(dtSource);

                                var doc = PdfFill_Manual();
                                if (doc != null)
                                {
                                    ZipArchiveEntry zipItem = zip.CreateEntry(row["Inventor"].ToString() + ".pdf");
                                    using (var originalStream = new MemoryStream(doc))
                                    {
                                        using (var entryStream = zipItem.Open())
                                        {
                                            originalStream.CopyTo(entryStream);
                                        }
                                    }
                                }
                            }
                        }
                        compressed = true;
                        compressedFiles = memoryStream.ToArray();
                    }
                    return compressedFiles;

                }
                else return PdfFill_Manual();
            }
            else
                return PdfFill_Xml();

        }

        protected byte[] PdfFill_Manual()
        {
            var fileMapDataSet = new DataSet();
            fileMapDataSet.ReadXml(MapFilePath);
            DataTable fieldMapDataTable = fileMapDataSet.Tables[0];

            var targetStream = new MemoryStream();
            var pdf = new PdfDocument(new PdfReader(SourceDocumentPath), new PdfWriter(targetStream));

            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
            IDictionary<String, PdfFormField> fields = form.GetFormFields();

            for (var i = 0; i <= DataSource.Tables.Count-1; i++)
            {
                for (var j = 0; j <= DataSource.Tables[i].Rows.Count - 1; j++)
                {
                    var mapTableView = GetPdfMapFields(fieldMapDataTable,i,j);

                    for (var k = 0; k <= mapTableView.Count-1; k++)
                    {
                        var pdfFieldName = mapTableView[k]["PdfField"].ToString();
                        var dbFieldName = mapTableView[k]["SourceField"].ToString();

                        fields.TryGetValue(pdfFieldName, out var toSet);

                        if (!string.IsNullOrEmpty(dbFieldName))
                        {
                            var dbFieldValue = DataSource.Tables[i].Rows[j][dbFieldName];

                            if (toSet?.GetType() == typeof(PdfTextFormField) || (toSet?.GetType() == typeof(PdfButtonFormField) && (dbFieldValue.ToString().ToLower()=="yes" || dbFieldValue.ToString().ToLower() == "on")))
                            {
                                toSet.SetValue(dbFieldValue == null ? "" : dbFieldValue.ToString());
                            }
                        }
                        else
                        {
                            toSet?.SetValue(mapTableView[k]["SourceValue"].ToString());
                        }
                    }

                }
            }

            //form.FlattenFields();
            pdf.Close();
            return targetStream.ToArray();
        }

        protected byte[] PdfFill_Xml()
        {
            XDocument doc = new XDocument();
            using (XmlWriter xw = doc.CreateWriter())
            {
                DataSource.WriteXml(xw);
                xw.Close();
            }

            XslCompiledTransform proc = new XslCompiledTransform();
            proc.Load(MapFilePath);

            string result;
            using (var sw = new StringWriterWithEncoding(Encoding.UTF8))
            {
                proc.Transform(doc.CreateNavigator(), null, sw); 
                result = sw.ToString();
            }

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(result);
            streamWriter.Flush();
            memoryStream.Position = 0;

            var targetStream = new MemoryStream();
            var pdf = new PdfDocument(new PdfReader(SourceDocumentPath), new PdfWriter(targetStream), new StampingProperties().UseAppendMode());

            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
            XfaForm xfa = form.GetXfaForm();

            xfa.FillXfaForm(memoryStream);
            xfa.Write(pdf);
            //RemoveUsageRights(pdf);
            pdf.Close();

            return targetStream.ToArray();
        }

        protected void RemoveUsageRights(PdfDocument pdfDoc)
        {
            PdfDictionary perms = pdfDoc.GetCatalog().GetPdfObject().GetAsDictionary(PdfName.Perms);
            if (perms == null)
            {
                return;
            }
            perms.Remove(new PdfName("UR"));
            perms.Remove(PdfName.UR3);
            if (perms.Size() == 0)
            {
                pdfDoc.GetCatalog().Remove(PdfName.Perms);
            }
        }

        protected DataView GetPdfMapFields(DataTable dtFieldMap,int tableNo, int rowNo)
        {
            var defaultView = dtFieldMap.DefaultView;
            defaultView.RowFilter = $"SourceTable={tableNo.ToString()} And SourceRow={rowNo.ToString()}";
            return defaultView;
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }

        public StringWriterWithEncoding(Encoding encoding)
        {
            Encoding = encoding;
        }
    }
}
