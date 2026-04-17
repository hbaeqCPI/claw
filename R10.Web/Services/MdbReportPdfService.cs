using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace LawPortal.Web.Services
{
    public class MdbReportPdfService
    {
        private static readonly DeviceRgb Yellow = new(255, 255, 0);
        private static readonly DeviceRgb Red = new(255, 0, 0);
        private static readonly DeviceRgb HeaderBg = new(200, 200, 200);
        private PdfFont _b = null!;
        private PdfFont _bi = null!;
        private PdfFont _r = null!;
        private Dictionary<string, string> _cn = new();
        private Dictionary<string, string> _ctd = new();

        public byte[] GenerateReport(MdbComparisonResult comparison, string releaseName, string year, string quarter,
            Dictionary<string, string>? countryNames = null, Dictionary<string, string>? caseTypeDescs = null)
        {
            _cn = countryNames ?? new(); _ctd = caseTypeDescs ?? new();
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf);
            doc.SetMargins(36, 40, 36, 40);
            _b = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            _bi = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLDOBLIQUE);
            _r = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var pfx = comparison.IsPatent ? "tblPat" : "tblTmk";
            var title = comparison.IsPatent ? "Patent Law Update" : "Trademark Law Update";
            var dueT = $"{pfx}CountryDue";
            var clT = $"{pfx}CountryLaw";
            var expT = comparison.IsPatent ? "tblPatCountryExp" : null;

            // ================================================================
            // HEADER
            // ================================================================
            doc.Add(new Paragraph(title).SetFont(_bi).SetFontSize(14).SetUnderline());
            doc.Add(new Paragraph("Country Law Updates").SetFont(_r).SetFontSize(11));
            HLine(doc, 1.5f);
            doc.Add(new Paragraph($"{year} — {quarter}").SetFont(_r).SetFontSize(11));

            // ================================================================
            // NEW ACTIONS (added CountryDue records)
            // ================================================================
            if (H(comparison, dueT) && comparison.TableDiffs[dueT].AddedRows.Any())
            {
                doc.Add(new Paragraph("\nNew Actions").SetFont(_b).SetFontSize(12).SetUnderline().SetMarginTop(8));
                WriteActionBlocks(doc, comparison.TableDiffs[dueT].AddedRows, comparison, clT, true, false);
            }

            // ================================================================
            // MODIFIED ACTIONS (modified CountryDue records)
            // ================================================================
            if (H(comparison, dueT) && comparison.TableDiffs[dueT].ModifiedRows.Any())
            {
                doc.Add(new Paragraph(title).SetFont(_bi).SetFontSize(14).SetUnderline().SetMarginTop(15));
                HLine(doc, 1.5f);
                doc.Add(new Paragraph("Modified Actions").SetFont(_b).SetFontSize(12).SetUnderline().SetMarginTop(5));
                WriteActionBlocks(doc, comparison.TableDiffs[dueT].ModifiedRows, comparison, clT, false, false);
            }

            // ================================================================
            // DELETED ACTIONS (deleted CountryDue records)
            // ================================================================
            if (H(comparison, dueT) && comparison.TableDiffs[dueT].DeletedRows.Any())
            {
                doc.Add(new Paragraph(title).SetFont(_bi).SetFontSize(14).SetUnderline().SetMarginTop(15));
                HLine(doc, 1.5f);
                doc.Add(new Paragraph("Deleted Actions:").SetFont(_b).SetFontSize(12).SetUnderline().SetMarginTop(5));
                WriteActionBlocks(doc, comparison.TableDiffs[dueT].DeletedRows, comparison, clT, false, true);
            }

            // ================================================================
            // COUNTRY LAW ADDED/MODIFIED (per Country, per CaseType)
            // ================================================================
            var hasClChanges = (H(comparison, clT) && (comparison.TableDiffs[clT].AddedRows.Any() || comparison.TableDiffs[clT].ModifiedRows.Any()));
            // Also include countries that have expiration changes
            if (hasClChanges || (expT != null && H(comparison, expT) && (comparison.TableDiffs[expT].AddedRows.Any() || comparison.TableDiffs[expT].ModifiedRows.Any())))
            {
                doc.Add(new Paragraph(title).SetFont(_bi).SetFontSize(14).SetUnderline().SetMarginTop(15));
                var hdr = new Paragraph().SetMarginTop(0);
                hdr.Add(Tx("Country Law Updates", _b, 13));
                hdr.Add(Tx($"                                                                        {year} - {quarter}", _r, 11));
                doc.Add(hdr);
                doc.Add(new Paragraph("Country Law Added/Modified").SetFont(_b).SetFontSize(12).SetUnderline());
                HLine(doc, 1.5f);

                // Collect all affected country+caseType
                var keys = new HashSet<string>();
                if (H(comparison, clT))
                    foreach (var r in comparison.TableDiffs[clT].AddedRows.Concat(comparison.TableDiffs[clT].ModifiedRows))
                        keys.Add($"{G(r, "Country")}|{G(r, "CaseType")}");
                if (expT != null && H(comparison, expT))
                    foreach (var r in comparison.TableDiffs[expT].AddedRows.Concat(comparison.TableDiffs[expT].ModifiedRows))
                        keys.Add($"{G(r, "Country")}|{G(r, "CaseType")}");

                var sorted = keys.Select(k => k.Split('|')).OrderBy(k => k[0]).ThenBy(k => k[1]);

                foreach (var pair in sorted)
                {
                    var country = pair[0]; var caseType = pair[1];

                    // Country: Albania (AL)
                    doc.Add(Pp(10).SetMarginTop(8)
                        .Add(Tx($"Country: {CN(country)} ({country})", _r, 10)));
                    // ORD - Utility Patent Filing
                    doc.Add(Pp(10).SetMarginLeft(5)
                        .Add(Tx($"{caseType} — {CD(caseType)}", _r, 10)));

                    // Expiration Terms Added
                    if (expT != null && H(comparison, expT))
                    {
                        var addedExps = comparison.TableDiffs[expT].AddedRows
                            .Where(e => G(e, "Country") == country && G(e, "CaseType") == caseType).ToList();
                        if (addedExps.Any())
                        {
                            doc.Add(Pp(9).SetMarginTop(3).Add(Tx("Expiration and Tax Terms Added", _b, 9)));
                            foreach (var e in addedExps)
                                doc.Add(Pp(8).SetMarginLeft(10)
                                    .Add(Tx($"Expiration   Based On: {G(e, "BasedOn")}   Terms: {G(e, "Yr")}-{G(e, "Mo")}   Effective: {G(e, "EffBasedOn")} {FD(e, "EffStartDate")} - {FD(e, "EffEndDate")}", _r, 8)));
                        }
                        var modExps = comparison.TableDiffs[expT].ModifiedRows
                            .Where(e => G(e, "Country") == country && G(e, "CaseType") == caseType).ToList();
                        if (modExps.Any())
                        {
                            doc.Add(Pp(9).SetMarginTop(3).Add(Tx("Expiration and Tax Terms Modified", _b, 9)));
                            foreach (var e in modExps)
                            {
                                var line = Pp(8).SetMarginLeft(10);
                                line.Add(Tx("Expiration   Based On: ", _r, 8));
                                AddFV(line, e, "BasedOn", G(e, "BasedOn"));
                                line.Add(Tx("   Terms: ", _r, 8));
                                var termsChanged = e.ChangedColumns.Intersect(new[] { "Yr", "Mo", "Dy" }).Any();
                                var terms = $"{G(e, "Yr")}-{G(e, "Mo")}-{G(e, "Dy")}";
                                line.Add(termsChanged ? Tx(terms, _r, 8).SetBackgroundColor(Yellow) : Tx(terms, _r, 8));
                                doc.Add(line);
                            }
                        }
                    }

                    // Law Actions for this country/caseType (from modified dues that match)
                    if (H(comparison, dueT))
                    {
                        var relDues = comparison.TableDiffs[dueT].AddedRows.Concat(comparison.TableDiffs[dueT].ModifiedRows)
                            .Where(d => G(d, "Country") == country && G(d, "CaseType") == caseType).ToList();
                        if (relDues.Any())
                        {
                            doc.Add(Pp(9).SetMarginTop(3).Add(Tx("Law Actions", _b, 9)));
                            foreach (var row in relDues.OrderBy(r => G(r, "ActionDue")))
                            {
                                var line = Pp(8).SetMarginLeft(10);
                                line.Add(Tx("Action: ", _b, 8));
                                var adChanged = row.ChangedColumns.Intersect(new[] { "ActionDue", "Indicator" }).Any() || row.OldValues == null;
                                var adText = $"{G(row, "ActionDue")} ({G(row, "Indicator")})";
                                line.Add(adChanged ? Tx(adText, _r, 8).SetBackgroundColor(Yellow) : Tx(adText, _r, 8));

                                line.Add(Tx("   Based On: ", _b, 8));
                                AddFV(line, row, "BasedOn", G(row, "BasedOn"));

                                line.Add(Tx("   Terms: ", _b, 8));
                                var terms = $"{G(row, "Yr")}-{G(row, "Mo")}-{G(row, "Dy")}";
                                var tChanged = row.ChangedColumns.Intersect(new[] { "Yr", "Mo", "Dy" }).Any();
                                line.Add(tChanged ? Tx(terms, _r, 8).SetBackgroundColor(Yellow) : Tx(terms, _r, 8));

                                var from = FD(row, "EffStartDate"); var to = FD(row, "EffEndDate");
                                if (!string.IsNullOrEmpty(from) || !string.IsNullOrEmpty(to) || !string.IsNullOrEmpty(G(row, "EffBasedOn")))
                                {
                                    line.Add(Tx($"   Eff: {G(row, "EffBasedOn")} ", _r, 8));
                                    AddFV(line, row, "EffStartDate", from);
                                    line.Add(Tx(" - ", _r, 8));
                                    AddFV(line, row, "EffEndDate", to);
                                }
                                doc.Add(line);
                            }
                        }
                    }

                    // Law Highlights (Remarks)
                    if (H(comparison, clT))
                    {
                        var clRow = comparison.TableDiffs[clT].AddedRows.Concat(comparison.TableDiffs[clT].ModifiedRows)
                            .FirstOrDefault(r => G(r, "Country") == country && G(r, "CaseType") == caseType);
                        if (clRow != null && !string.IsNullOrWhiteSpace(G(clRow, "Remarks")))
                        {
                            doc.Add(Pp(9).SetMarginTop(4).Add(Tx("Law Highlights ", _b, 9)));
                            var remarks = G(clRow, "Remarks");
                            bool isModified = clRow.OldValues != null && clRow.ChangedColumns.Contains("Remarks");
                            if (isModified)
                            {
                                var old = clRow.OldValues!.ContainsKey("Remarks") ? clRow.OldValues["Remarks"]?.ToString() ?? "" : "";
                                doc.Add(LineDiff(old, remarks).SetMarginLeft(10).SetMarginBottom(3));
                            }
                            else
                            {
                                doc.Add(Pp(8).SetMarginLeft(10).SetMarginBottom(3).Add(Tx(remarks, _r, 8)));
                            }
                        }
                    }

                    HLine(doc, 0.5f);
                }
            }

            // ================================================================
            // STRUCTURAL: Areas, CaseTypes, Designations
            // ================================================================
            AddStructural(doc, comparison, pfx);

            // ================================================================
            // COUNTRY LAW DELETED
            // ================================================================
            if (H(comparison, clT) && comparison.TableDiffs[clT].DeletedRows.Any())
            {
                doc.Add(new Paragraph("Country Law Deleted").SetFont(_b).SetFontSize(12).SetUnderline().SetMarginTop(12));
                foreach (var r in comparison.TableDiffs[clT].DeletedRows.OrderBy(r => G(r, "Country")).ThenBy(r => G(r, "CaseType")))
                    doc.Add(Pp(9).SetMarginLeft(10).Add(Tx($"Country: {CN(G(r, "Country"))} ({G(r, "Country")}) — {CD(G(r, "CaseType"))}", _r, 9)));
            }

            doc.Close();
            return ms.ToArray();
        }

        // ================================================================
        // Write Action Blocks (New / Modified / Deleted)
        // Groups by Country → ActionType, shows table of ActionDue|Yr|Mo|Dy|Indicator
        // ================================================================
        private void WriteActionBlocks(Document doc, List<RowDiff> rows, MdbComparisonResult comparison, string clT, bool isNew, bool isDeleted)
        {
            var byCountryAT = rows
                .GroupBy(r => $"{G(r, "Country")}|{G(r, "ActionType")}")
                .OrderBy(g => g.Key);

            foreach (var group in byCountryAT)
            {
                var first = group.First();
                var country = G(first, "Country");
                var actionType = G(first, "ActionType");
                var cpi = first.Values.ContainsKey("CPIAction") && first.Values["CPIAction"] is bool bv && bv;

                // Action Type: NAME          Office Action: No
                var atP = Pp(10).SetMarginLeft(15).SetMarginTop(8);
                atP.Add(Tx("Action Type: ", _r, 10));
                if (isDeleted)
                    atP.Add(Tx($"{actionType} - DELETE", _b, 10).SetBackgroundColor(Red).SetFontColor(ColorConstants.WHITE));
                else
                    atP.Add(Tx(actionType, _b, 10).SetBackgroundColor(isNew ? Yellow : null));
                atP.Add(Tx($"          Office Action: {(cpi ? "Yes" : "No")}", _r, 10));
                doc.Add(atP);

                // Country: EP    European Patent Convention
                doc.Add(Pp(10).SetMarginLeft(25)
                    .Add(Tx($"Country: {country}     {CN(country)}", _r, 10)));

                // Action Due table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 40, 8, 8, 8, 18 }))
                    .UseAllAvailableWidth().SetFontSize(9).SetMarginLeft(20).SetMarginTop(5);
                table.AddHeaderCell(HC("Action Due")); table.AddHeaderCell(HC("Yr")); table.AddHeaderCell(HC("Mo")); table.AddHeaderCell(HC("Dy")); table.AddHeaderCell(HC("Indicator"));

                foreach (var row in group.OrderBy(r => G(r, "ActionDue")))
                {
                    bool highlight = isNew; // New actions: all yellow. Modified/Deleted: only changed fields
                    if (!isNew && !isDeleted)
                    {
                        // Modified: highlight only changed fields
                        AC(table, G(row, "ActionDue"), row.ChangedColumns.Contains("ActionDue"));
                        AC(table, G(row, "Yr"), row.ChangedColumns.Contains("Yr"));
                        AC(table, G(row, "Mo"), row.ChangedColumns.Contains("Mo"));
                        AC(table, G(row, "Dy"), row.ChangedColumns.Contains("Dy"));
                        AC(table, G(row, "Indicator"), row.ChangedColumns.Contains("Indicator"));
                    }
                    else
                    {
                        AC(table, G(row, "ActionDue"), highlight);
                        AC(table, G(row, "Yr"), highlight);
                        AC(table, G(row, "Mo"), highlight);
                        AC(table, G(row, "Dy"), highlight);
                        AC(table, G(row, "Indicator"), highlight);
                    }
                }
                doc.Add(table);

                // Remarks
                string remarks = "";
                if (H(comparison, clT))
                {
                    var clRow = comparison.TableDiffs[clT].AddedRows.Concat(comparison.TableDiffs[clT].ModifiedRows)
                        .FirstOrDefault(r => G(r, "Country") == country && G(r, "CaseType") == G(first, "CaseType"));
                    if (clRow != null) remarks = G(clRow, "Remarks");
                }
                if (string.IsNullOrWhiteSpace(remarks))
                    remarks = G(first, "Remarks"); // fallback to the due record if available

                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    var rp = Pp(8).SetMarginLeft(20).SetMarginTop(3);
                    rp.Add(Tx("Remarks: ", _b, 8));
                    if (isNew)
                        rp.Add(Tx(remarks, _r, 8).SetBackgroundColor(Yellow));
                    else
                        rp.Add(Tx(remarks, _r, 8));
                    doc.Add(rp);
                }

                // Follow Up
                var effBasedOn = G(first, "EffBasedOn");
                if (!string.IsNullOrWhiteSpace(effBasedOn))
                {
                    doc.Add(Pp(8).SetMarginLeft(20).SetMarginTop(2)
                        .Add(Tx($"Follow Up Term: {G(first, "Mo")} Month(s) / {G(first, "Dy")} Day(s)  Follow Up Based On: {effBasedOn}", _r, 8)));
                }

                // Created/Updated dates for modified
                if (!isNew && !isDeleted)
                {
                    var created = G(first, "DateCreated"); var updated = G(first, "LastUpdate");
                    if (!string.IsNullOrEmpty(created) || !string.IsNullOrEmpty(updated))
                    {
                        var dates = Pp(8).SetMarginLeft(20).SetMarginTop(2);
                        if (!string.IsNullOrEmpty(created))
                        {
                            if (DateTime.TryParse(created, out var cd)) dates.Add(Tx($"Created: {cd:d-MMM-yyyy}", _r, 8));
                        }
                        if (!string.IsNullOrEmpty(updated))
                        {
                            if (DateTime.TryParse(updated, out var ud)) dates.Add(Tx($"     Updated: {ud:d-MMM-yyyy}", _r, 8));
                        }
                        doc.Add(dates);
                    }
                }

                HLine(doc, 0.5f);
            }
        }

        // ================================================================
        // STRUCTURAL CHANGES
        // ================================================================
        private void AddStructural(Document doc, MdbComparisonResult c, string pfx)
        {
            void Sec(string tbl, string label, bool added)
            {
                if (!H(c, tbl)) return;
                var rows = added ? c.TableDiffs[tbl].AddedRows : c.TableDiffs[tbl].DeletedRows;
                if (!rows.Any()) return;
                doc.Add(new Paragraph(label).SetFont(_b).SetFontSize(11).SetMarginTop(8));
                if (tbl.Contains("AreaCountry"))
                    foreach (var g in rows.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                        doc.Add(Pp(9).SetMarginLeft(10).Add(Tx($"{g.Key}: {string.Join(", ", g.Select(r => $"{CN(G(r, "Country"))} ({G(r, "Country")})").OrderBy(x => x))}", _r, 9)));
                else if (tbl.Contains("CaseType") && !tbl.Contains("Des"))
                    foreach (var r in rows.OrderBy(r => G(r, "CaseType")))
                        doc.Add(Pp(9).SetMarginLeft(10).Add(Tx($"{G(r, "CaseType")} — {G(r, "Description")}", _r, 9)));
                else if (tbl.Contains("DesCaseType"))
                    foreach (var g in rows.GroupBy(r => G(r, "IntlCode")).OrderBy(g => g.Key))
                    {
                        doc.Add(Pp(9).SetMarginLeft(10).Add(Tx($"Organization/Union: {g.Key}", _b, 9)));
                        foreach (var r in g)
                            doc.Add(Pp(8).SetMarginLeft(20).Add(Tx($"{(added ? "Can be designated in" : "designated in")} {CN(G(r, "DesCountry"))} ({G(r, "DesCountry")})  Type: {G(r, "CaseType")} as {G(r, "DesCaseType")}", _r, 8)));
                    }
            }
            Sec($"{pfx}AreaCountryDelete", "Area Countries Deleted", true);
            Sec($"{pfx}AreaCountry", "Area Countries Added", true);
            Sec($"{pfx}AreaCountry", "Area Countries Removed", false);
            Sec($"{pfx}CaseType", "Case Types Added", true);
            Sec($"{pfx}CaseType", "Case Types Deleted", false);
            Sec($"{pfx}DesCaseType", "Designation Deleted", false);
            Sec($"{pfx}DesCaseType", "Designation Added", true);
        }

        // ================================================================
        // LINE DIFF for Law Highlights
        // ================================================================
        private Paragraph LineDiff(string old, string nw)
        {
            var p = Pp(8);
            var oL = (old ?? "").Split('\n');
            var nL = (nw ?? "").Split('\n');
            int c = 0;
            while (c < oL.Length && c < nL.Length && oL[c].TrimEnd() == nL[c].TrimEnd()) c++;
            for (int i = 0; i < c; i++) { if (i > 0) p.Add(Tx("\n", _r, 8)); p.Add(Tx(nL[i], _r, 8)); }
            for (int i = c; i < nL.Length; i++) { if (i > 0 || c > 0) p.Add(Tx("\n", _r, 8)); p.Add(Tx(nL[i], _r, 8).SetBackgroundColor(Yellow)); }
            return p;
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private static string G(RowDiff r, string c) => r.Values.ContainsKey(c) ? r.Values[c]?.ToString() ?? "" : "";
        private string CN(string c) => _cn.ContainsKey(c) ? _cn[c] : c;
        private string CD(string c) => _ctd.ContainsKey(c) ? _ctd[c] : c;
        private static string FD(RowDiff r, string c) { var v = G(r, c); return DateTime.TryParse(v, out var d) ? d.ToString("M/d/yyyy") : v; }
        private static bool H(MdbComparisonResult c, string t) => c.TableDiffs.ContainsKey(t);
        private Paragraph Pp(float sz) => new Paragraph().SetFont(_r).SetFontSize(sz);
        private Text Tx(string s, PdfFont f, float sz) => new Text(s ?? "").SetFont(f).SetFontSize(sz);
        private void HLine(Document d, float w) => d.Add(new Paragraph("").SetBorderBottom(new SolidBorder(ColorConstants.BLACK, w)).SetMarginTop(1).SetMarginBottom(1));

        private Cell HC(string text) => new Cell().Add(new Paragraph(text).SetFont(_b).SetFontSize(8))
            .SetBackgroundColor(HeaderBg).SetBorder(new SolidBorder(0.5f)).SetPadding(3);

        private void AC(Table t, string text, bool highlight)
        {
            var cell = new Cell().Add(new Paragraph(text ?? "").SetFont(_r).SetFontSize(8))
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
            if (highlight) cell.SetBackgroundColor(Yellow);
            t.AddCell(cell);
        }

        private void AddFV(Paragraph line, RowDiff row, string col, string val)
        {
            if (row.ChangedColumns.Contains(col))
            {
                if (row.OldValues != null && row.OldValues.ContainsKey(col))
                {
                    var old = row.OldValues[col]?.ToString() ?? "";
                    if (DateTime.TryParse(old, out var dt)) old = dt.ToString("M/d/yyyy");
                    if (!string.IsNullOrEmpty(old) && old != val)
                    {
                        line.Add(Tx(old, _r, 8).SetFontColor(ColorConstants.GRAY).SetLineThrough());
                        line.Add(Tx(" → ", _r, 8));
                    }
                }
                line.Add(Tx(val, _r, 8).SetBackgroundColor(Yellow));
            }
            else
                line.Add(Tx(val, _r, 8));
        }
    }
}
