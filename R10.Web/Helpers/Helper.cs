using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using R10.Web.Interfaces;

namespace R10.Web.Helpers
{
    public static class Helper
    {
        public static string BuildSortableLink(string column, string label)
        {
            var link = $"<span class='k-link' data-field='{column}' data-label='{label}'><a href='#' class='k-link'>{label}</a></span>";
            return link;
        }

        public static void ExtractPdfPage(Stream sourcePdf, int[] pageNumbersToExtract, string outPdf)
        {
            try
            {
                sourcePdf.Position = 0;
                var reader = new iTextSharp.text.pdf.PdfReader(sourcePdf);
                var doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));

                using (var outputStream = new FileStream(outPdf, FileMode.Create)) {
                    var pdfCopy = new iTextSharp.text.pdf.PdfCopy(doc, outputStream);
                    doc.Open();
                    foreach (int pageNum in pageNumbersToExtract)
                    {
                        var page = pdfCopy.GetImportedPage(reader, pageNum);
                        pdfCopy.AddPage(page);
                    }
                    doc.Close();
                    reader.Close();
                    pdfCopy.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static int ExtractPdfPageCount(Stream sourcePdf)
        {
            try
            {
                var reader = new iTextSharp.text.pdf.PdfReader(sourcePdf);
                sourcePdf.Position = 0;
                return reader.NumberOfPages;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int LevenshteinDistance_DiffCompute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static (DateTime StartDate, DateTime EndDate) GetFiscalDates(string? fiscalCalendarYearStart, string? fiscalCalendarYearEnd)
        {
            var today = DateTime.Now.Date;
            var startDate = new DateTime(today.Year, 1, 1);
            var endDate = new DateTime(today.Year, 12, 31);

            if (!string.IsNullOrEmpty(fiscalCalendarYearStart) && !string.IsNullOrEmpty(fiscalCalendarYearEnd))
            {
                var fiscalStartDate = DateTime.ParseExact(fiscalCalendarYearStart + "/" + DateTime.Today.Year, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var fiscalEndDate = DateTime.ParseExact(fiscalCalendarYearEnd + "/" + DateTime.Today.Year, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                if (fiscalStartDate > fiscalEndDate)
                {
                    if (today <= fiscalEndDate)
                    {
                        startDate = fiscalStartDate.AddYears(-1);
                        endDate = fiscalEndDate;
                    }
                    else
                    {
                        startDate = fiscalStartDate;
                        endDate = fiscalEndDate.AddYears(1);
                    }
                }
                else
                {
                    startDate = fiscalStartDate;
                    endDate = fiscalEndDate;
                }
            }
            return (startDate, endDate);
        }

        public static List<Tuple<int, DateTime, DateTime>> GetQuarters(DateTime fiscalYearStart, DateTime fiscalYearEnd)
        {
            List<Tuple<int, DateTime, DateTime>> quarters = new List<Tuple<int, DateTime, DateTime>>();

            DateTime quarterStart = fiscalYearStart;
            for (int i = 1; i <= 4; i++)
            {
                // Calculate the end date of the quarter by adding 3 months
                DateTime quarterEnd = quarterStart.AddMonths(3).AddDays(-1);

                // For the last quarter, ensure the end date is the fiscal year end date
                if (i == 4)
                {
                    quarterEnd = fiscalYearEnd;
                }

                quarters.Add(new Tuple<int, DateTime, DateTime>(i, quarterStart, quarterEnd));
        
                // The start of the next quarter is the day after the current one ends
                quarterStart = quarterEnd.AddDays(1);
            }

            return quarters;
        }
    }


}
