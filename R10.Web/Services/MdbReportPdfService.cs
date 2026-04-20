using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace LawPortal.Web.Services
{
    public class MdbReportPdfService
    {
        private static readonly DeviceRgb Yellow = new(255, 255, 0);
        private static readonly DeviceRgb Red = new(220, 50, 50);
        private static readonly DeviceRgb HdrBg = new(235, 235, 235);

        private PdfFont _b = null!, _bi = null!, _i = null!, _r = null!;
        private Dictionary<string, string> _cn = new(), _ctd = new();

        public byte[] GenerateReport(MdbComparisonResult comp, string name, string year, string qtr,
            Dictionary<string, string>? cn = null, Dictionary<string, string>? ctd = null)
        {
            _cn = cn ?? new();
            _ctd = ctd ?? new();

            using var ms = new MemoryStream();
            using var w = new PdfWriter(ms);
            using var pdf = new PdfDocument(w);

            _b = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            _bi = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLDOBLIQUE);
            _i = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
            _r = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var title = comp.IsPatent ? "Patent Law Update" : "Trademark Law Update";
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new RunningHeader(title, _bi));

            using var doc = new Document(pdf);
            doc.SetMargins(60, 54, 45, 54);

            var pfx = comp.IsPatent ? "tblPat" : "tblTmk";
            var dueT = $"{pfx}CountryDue";
            var clT = $"{pfx}CountryLaw";
            var expT = comp.IsPatent ? "tblPatCountryExp" : null;
            var expDelT = comp.IsPatent ? "tblPatCountryExpDelete" : null;

            WriteTitle(doc, year, qtr);

            if (comp.IsPatent)
                WriteManualUpdates(doc, comp, dueT, clT);
            else
                WriteStructural(doc, comp, pfx);

            WriteCountryLawAddedModified(doc, comp, year, qtr, clT, dueT, expT, expDelT);

            if (comp.IsPatent)
                WriteStructural(doc, comp, pfx);

            WriteCountryLawDeleted(doc, comp, clT);

            doc.Close();
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════
        // RUNNING HEADER — italic-bold title + underline on every page
        // ═══════════════════════════════════════════════════════════════
        private class RunningHeader : IEventHandler
        {
            private readonly string _title;
            private readonly PdfFont _font;
            public RunningHeader(string title, PdfFont font) { _title = title; _font = font; }
            public void HandleEvent(Event ev)
            {
                var de = (PdfDocumentEvent)ev;
                var page = de.GetPage();
                var size = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                float left = 54f, right = size.GetWidth() - 54f, top = size.GetTop() - 36f;
                canvas.BeginText()
                    .SetFontAndSize(_font, 11)
                    .MoveText(left, top)
                    .ShowText(_title)
                    .EndText();
                canvas.SetLineWidth(0.8f)
                    .MoveTo(left, top - 4)
                    .LineTo(right, top - 4)
                    .Stroke();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // TITLE: "Country Law Updates" + "2026 – 1"
        // ═══════════════════════════════════════════════════════════════
        private void WriteTitle(Document doc, string year, string qtr)
        {
            var t = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                .UseAllAvailableWidth().SetMarginTop(8);
            t.AddCell(new Cell().Add(
                P(22).SetFont(_b).Add(T("Country Law Updates", _b, 22))
            ).SetBorder(Border.NO_BORDER));
            t.AddCell(new Cell().Add(
                P(18).SetTextAlignment(TextAlignment.RIGHT).SetPaddingTop(6)
                    .Add(T($"{year} – {qtr}", _r, 18))
            ).SetBorder(Border.NO_BORDER));
            doc.Add(t);
        }

        // ═══════════════════════════════════════════════════════════════
        // PATENT: MANUAL UPDATES (Action Type changes)
        // ═══════════════════════════════════════════════════════════════
        private void WriteManualUpdates(Document doc, MdbComparisonResult comp, string dueT, string clT)
        {
            if (!H(comp, dueT)) return;
            var diff = comp.TableDiffs[dueT];
            if (!diff.AddedRows.Any() && !diff.ModifiedRows.Any() && !diff.DeletedRows.Any()) return;

            doc.Add(P(18).SetFont(_r).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(20)
                .Add(T("Manual Updates", _r, 18)));

            doc.Add(P(10).SetMarginTop(14)
                .Add(T("Records in the Action Type table under the Auxiliary or Maintenance Menu in your system are not automatically modified. They will need to be adjusted by a person responsible for data entry.", _r, 10)));
            doc.Add(P(10).SetMarginTop(6)
                .Add(T("Go to the Auxiliary or Maintenance Menu in your system and add, modify, or delete the Action Types below as applicable.", _r, 10)));
            doc.Add(P(10).SetMarginTop(6)
                .Add(T("Please contact CPi at ", _r, 10))
                .Add(T("countrylaw@computerpackages.com", _r, 10).SetFontColor(ColorConstants.BLUE).SetUnderline())
                .Add(T(" with any questions.", _r, 10)));

            if (diff.AddedRows.Any())
            {
                doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T("New Actions", _b, 11)));
                WriteActionTypeSection(doc, diff.AddedRows, comp, clT, "new");
            }
            if (diff.ModifiedRows.Any())
            {
                doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T("Modified Actions", _b, 11)));
                WriteActionTypeSection(doc, diff.ModifiedRows, comp, clT, "mod");
            }
            if (diff.DeletedRows.Any())
            {
                doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T("Deleted Actions:", _b, 11)));
                WriteActionTypeSection(doc, diff.DeletedRows, comp, clT, "del");
            }
        }

        // One Action Type block (ActionType → Country → rows)
        private void WriteActionTypeSection(Document doc, List<RowDiff> rows, MdbComparisonResult comp, string clT, string mode)
        {
            bool isNew = mode == "new", isDel = mode == "del";

            foreach (var atGrp in rows.GroupBy(r => G(r, "ActionType")).OrderBy(g => g.Key))
            {
                var at = atGrp.Key;

                var ap = P(10).SetMarginLeft(40).SetMarginTop(10);
                ap.Add(T("Action Type: ", _r, 10));
                if (isDel) ap.Add(T($"{at} - DELETE", _b, 10).SetBackgroundColor(Red).SetFontColor(ColorConstants.WHITE));
                else ap.Add(T(at, _b, 10).SetBackgroundColor(isNew ? Yellow : (Color?)null));
                doc.Add(ap);

                foreach (var cGrp in atGrp.GroupBy(r => G(r, "Country")).OrderBy(g => CN(g.Key)))
                {
                    var country = cGrp.Key;
                    var cp = P(10).SetMarginLeft(60).SetMarginTop(2);
                    cp.Add(T("Country: ", _r, 10));
                    if (isNew)
                    {
                        cp.Add(T(country, _r, 10).SetBackgroundColor(Yellow));
                        cp.Add(T($"     ", _r, 10));
                        cp.Add(T(CN(country), _r, 10).SetBackgroundColor(Yellow));
                    }
                    else cp.Add(T($"{country}     {CN(country)}", _r, 10));
                    doc.Add(cp);

                    // 5-col table: Action Due | Yr | Mo | Dy | Indicator
                    var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 40, 8, 8, 8, 20 }))
                        .UseAllAvailableWidth().SetMarginLeft(60).SetMarginTop(6).SetFontSize(9);
                    tbl.AddHeaderCell(HC("Action Due"));
                    tbl.AddHeaderCell(HC("Yr"));
                    tbl.AddHeaderCell(HC("Mo"));
                    tbl.AddHeaderCell(HC("Dy"));
                    tbl.AddHeaderCell(HC("Indicator"));

                    foreach (var row in cGrp.OrderBy(r => G(r, "ActionDue")))
                    {
                        if (isNew)
                        {
                            YC(tbl, G(row, "ActionDue")); YC(tbl, G(row, "Yr")); YC(tbl, G(row, "Mo"));
                            YC(tbl, G(row, "Dy")); YC(tbl, G(row, "Indicator"));
                        }
                        else if (isDel)
                        {
                            DC(tbl, G(row, "ActionDue")); DC(tbl, G(row, "Yr")); DC(tbl, G(row, "Mo"));
                            DC(tbl, G(row, "Dy")); DC(tbl, G(row, "Indicator"));
                        }
                        else
                        {
                            MC(tbl, row, "ActionDue"); MC(tbl, row, "Yr"); MC(tbl, row, "Mo");
                            MC(tbl, row, "Dy"); MC(tbl, row, "Indicator");
                        }
                    }
                    doc.Add(tbl);

                    // Remarks
                    var cf = cGrp.First();
                    string rem = G(cf, "Remarks");
                    if (string.IsNullOrWhiteSpace(rem) && H(comp, clT))
                    {
                        var cl = comp.TableDiffs[clT].AddedRows.Concat(comp.TableDiffs[clT].ModifiedRows)
                            .FirstOrDefault(r => G(r, "Country") == country && G(r, "CaseType") == G(cf, "CaseType"));
                        if (cl != null) rem = G(cl, "Remarks");
                    }
                    if (!string.IsNullOrWhiteSpace(rem))
                    {
                        var rt = new Table(UnitValue.CreatePercentArray(new float[] { 12, 88 }))
                            .UseAllAvailableWidth().SetMarginLeft(60).SetMarginTop(6);
                        rt.AddCell(new Cell().Add(P(9).Add(T("Remarks:", _r, 9)))
                            .SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetPaddingRight(4));
                        var remCell = new Cell().SetBorder(Border.NO_BORDER);
                        var remP = P(9);
                        if (isNew) remP.Add(T(rem, _r, 9).SetBackgroundColor(Yellow));
                        else remP.Add(T(rem, _r, 9));
                        remCell.Add(remP);
                        rt.AddCell(remCell);
                        doc.Add(rt);
                    }

                    // Follow-up
                    var eff = G(cf, "EffBasedOn");
                    if (!string.IsNullOrWhiteSpace(eff))
                    {
                        doc.Add(P(9).SetMarginLeft(90).SetMarginTop(4)
                            .Add(T("Follow Up Action:", _r, 9)));
                        doc.Add(P(9).SetMarginLeft(90)
                            .Add(T($"Follow Up Term: {G(cf, "Mo")} Month(s) / {G(cf, "Dy")} Day(s)", _r, 9)));
                        doc.Add(P(9).SetMarginLeft(90)
                            .Add(T($"Follow Up Based On: {eff}", _r, 9)));
                    }
                    HL(doc, 0.3f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // TRADEMARK STRUCTURAL: Areas, Case Types, Designations
        // ═══════════════════════════════════════════════════════════════
        private void WriteStructural(Document doc, MdbComparisonResult c, string pfx)
        {
            var acTbl = $"{pfx}AreaCountry";
            var acDelTbl = $"{pfx}AreaCountryDelete";
            var ctTbl = $"{pfx}CaseType";
            var desTbl = $"{pfx}DesCaseType";
            var desDelTbl = $"{pfx}DesCaseTypeDelete";

            // Area Countries Deleted
            var acDel = new List<RowDiff>();
            if (H(c, acTbl)) acDel.AddRange(c.TableDiffs[acTbl].DeletedRows);
            if (H(c, acDelTbl)) acDel.AddRange(c.TableDiffs[acDelTbl].AddedRows);
            if (acDel.Any())
            {
                SectionHeader(doc, "Area Countries Deleted");
                foreach (var g in acDel.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    doc.Add(P(10).SetFont(_b).SetMarginTop(4).Add(T(g.Key, _b, 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        doc.Add(P(9).SetMarginLeft(30).Add(T($"{CN(G(r, "Country"))} ({G(r, "Country")})", _r, 9)));
                }
            }

            // Area Countries Added
            if (H(c, acTbl) && c.TableDiffs[acTbl].AddedRows.Any())
            {
                SectionHeader(doc, "Area Countries Added");
                foreach (var g in c.TableDiffs[acTbl].AddedRows.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    doc.Add(P(10).SetFont(_b).SetMarginTop(4).Add(T(g.Key, _b, 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        doc.Add(P(9).SetMarginLeft(30).Add(T($"{CN(G(r, "Country"))} ({G(r, "Country")})", _r, 9)));
                }
            }

            // Case Types Added
            if (H(c, ctTbl) && c.TableDiffs[ctTbl].AddedRows.Any())
            {
                SectionHeader(doc, "Case Types Added");
                foreach (var r in c.TableDiffs[ctTbl].AddedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "CaseType"), _r, 9))).SetBorder(Border.NO_BORDER).SetPaddingLeft(15));
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "Description"), _r, 9))).SetBorder(Border.NO_BORDER));
                    doc.Add(t);
                }
            }

            // Case Types Deleted
            if (H(c, ctTbl) && c.TableDiffs[ctTbl].DeletedRows.Any())
            {
                SectionHeader(doc, "Case Types Deleted");
                foreach (var r in c.TableDiffs[ctTbl].DeletedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "CaseType"), _r, 9))).SetBorder(Border.NO_BORDER).SetPaddingLeft(15));
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "Description"), _r, 9))).SetBorder(Border.NO_BORDER));
                    doc.Add(t);
                }
            }

            // Designation Deleted
            var desDel = new List<RowDiff>();
            if (H(c, desTbl)) desDel.AddRange(c.TableDiffs[desTbl].DeletedRows);
            if (H(c, desDelTbl)) desDel.AddRange(c.TableDiffs[desDelTbl].AddedRows);
            if (desDel.Any())
            {
                SectionHeader(doc, "Designation Deleted");
                WriteDesignations(doc, desDel, added: false);
            }

            // Designation Added
            if (H(c, desTbl) && c.TableDiffs[desTbl].AddedRows.Any())
            {
                SectionHeader(doc, "Designation Added");
                WriteDesignations(doc, c.TableDiffs[desTbl].AddedRows, added: true);
            }
        }

        private void WriteDesignations(Document doc, List<RowDiff> rows, bool added)
        {
            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .UseAllAvailableWidth().SetMarginTop(4);
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Organization/Union", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Type of filing (Case Type)", _b, 9))).SetBorder(Border.NO_BORDER));

            foreach (var ioGrp in rows.GroupBy(r => G(r, "IntlCode")).OrderBy(g => CN(g.Key)))
            {
                foreach (var ctGrp in ioGrp.GroupBy(r => G(r, "CaseType")).OrderBy(g => g.Key))
                {
                    var ct = ctGrp.Key;
                    tbl.AddCell(new Cell().Add(P(9).SetFont(_b).SetMarginTop(4)
                        .Add(T($"{CN(ioGrp.Key)} ({ioGrp.Key})", _b, 9))).SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).SetFont(_b).SetMarginTop(4)
                        .Add(T($"{ct} - {CD(ct)}", _b, 9))).SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).Add(T(added ? "Can be designated in" : "designated in", _r, 9)))
                        .SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).Add(T("as", _r, 9))).SetBorder(Border.NO_BORDER));
                    foreach (var r in ctGrp.OrderBy(r => CN(G(r, "DesCountry"))))
                    {
                        tbl.AddCell(new Cell().Add(P(9).Add(T($"{CN(G(r, "DesCountry"))} ({G(r, "DesCountry")})", _r, 9)))
                            .SetBorder(Border.NO_BORDER));
                        tbl.AddCell(new Cell().Add(P(9).Add(T(G(r, "DesCaseType"), _r, 9)))
                            .SetBorder(Border.NO_BORDER));
                    }
                }
            }
            doc.Add(tbl);
        }

        private void SectionHeader(Document doc, string text) =>
            doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T(text, _b, 11)));

        // ═══════════════════════════════════════════════════════════════
        // COUNTRY LAW ADDED/MODIFIED
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountryLawAddedModified(Document doc, MdbComparisonResult comp, string year, string qtr,
            string clT, string dueT, string? expT, string? expDelT)
        {
            var keys = new HashSet<(string, string)>();
            void Add(IEnumerable<RowDiff>? rows)
            {
                if (rows == null) return;
                foreach (var r in rows) keys.Add((G(r, "Country"), G(r, "CaseType")));
            }
            if (H(comp, clT)) { Add(comp.TableDiffs[clT].AddedRows); Add(comp.TableDiffs[clT].ModifiedRows); }
            if (H(comp, dueT)) { Add(comp.TableDiffs[dueT].AddedRows); Add(comp.TableDiffs[dueT].ModifiedRows); }
            if (expT != null && H(comp, expT)) { Add(comp.TableDiffs[expT].AddedRows); Add(comp.TableDiffs[expT].ModifiedRows); }
            if (expDelT != null && H(comp, expDelT)) Add(comp.TableDiffs[expDelT].AddedRows);

            if (!keys.Any()) return;

            doc.Add(new AreaBreak());
            WriteTitle(doc, year, qtr);
            doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(6)
                .Add(T("Country Law Added/Modified", _b, 11)));

            var newClKeys = new HashSet<(string, string)>();
            if (H(comp, clT))
                foreach (var r in comp.TableDiffs[clT].AddedRows)
                    newClKeys.Add((G(r, "Country"), G(r, "CaseType")));

            foreach (var (country, caseType) in keys.OrderBy(k => CN(k.Item1)).ThenBy(k => k.Item2))
            {
                bool isNewBlock = newClKeys.Contains((country, caseType));
                WriteCountryBlock(doc, comp, country, caseType, isNewBlock, clT, dueT, expT, expDelT);
            }
        }

        private void WriteCountryBlock(Document doc, MdbComparisonResult comp,
            string country, string caseType, bool isNewBlock,
            string clT, string dueT, string? expT, string? expDelT)
        {
            // Country + CaseType header line
            var hdr = new Table(UnitValue.CreatePercentArray(new float[] { 55, 45 }))
                .UseAllAvailableWidth().SetMarginTop(12);
            var lp = P(11).Add(T($"Country: {CN(country)} ({country})", _bi, 11));
            if (isNewBlock) lp.SetBackgroundColor(Yellow);
            hdr.AddCell(new Cell().Add(lp).SetBorder(Border.NO_BORDER));
            var rp = P(11).SetTextAlignment(TextAlignment.RIGHT)
                .Add(T($"{caseType} - {CD(caseType)}", _bi, 11));
            if (isNewBlock) rp.SetBackgroundColor(Yellow);
            hdr.AddCell(new Cell().Add(rp).SetBorder(Border.NO_BORDER));
            doc.Add(hdr);

            // Expiration & Tax Terms (patents only)
            if (expT != null && H(comp, expT))
            {
                var ed = comp.TableDiffs[expT];
                var added = ed.AddedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                var modified = ed.ModifiedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                if (added.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T("Expiration and Tax Terms Added", _b, 10)));
                    WriteExpTable(doc, added, mode: "add");
                }
                if (modified.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T("Expiration and Tax Terms Modified", _b, 10)));
                    WriteExpTable(doc, modified, mode: "mod");
                }
            }
            if (expDelT != null && H(comp, expDelT))
            {
                var del = comp.TableDiffs[expDelT].AddedRows
                    .Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                if (del.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T("Expiration and Tax Terms Deleted", _b, 10)));
                    WriteExpTable(doc, del, mode: "del");
                }
            }

            // Law Actions
            if (H(comp, dueT))
            {
                var dd = comp.TableDiffs[dueT];
                var rel = dd.AddedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType)
                    .Concat(dd.ModifiedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType))
                    .ToList();
                if (rel.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6).Add(T("Law Actions", _b, 10)));
                    if (comp.IsPatent) WritePatentLawActions(doc, rel, isNewBlock);
                    else WriteTrademarkLawActions(doc, rel, isNewBlock);
                }
            }

            // Law Highlights (Remarks)
            if (H(comp, clT))
            {
                var cd = comp.TableDiffs[clT];
                var clRow = cd.AddedRows.Concat(cd.ModifiedRows)
                    .FirstOrDefault(r => G(r, "Country") == country && G(r, "CaseType") == caseType);
                if (clRow != null && !string.IsNullOrWhiteSpace(G(clRow, "Remarks")))
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(8)
                        .Add(T("Law Highlights", _b, 10)));
                    var remarks = G(clRow, "Remarks");
                    if (isNewBlock || clRow.OldValues == null || !clRow.ChangedColumns.Contains("Remarks"))
                    {
                        var rP = P(9).SetMarginLeft(20).SetMarginTop(3);
                        if (isNewBlock) rP.Add(T(remarks, _r, 9).SetBackgroundColor(Yellow));
                        else rP.Add(T(remarks, _r, 9));
                        doc.Add(rP);
                    }
                    else
                    {
                        var old = clRow.OldValues.ContainsKey("Remarks")
                            ? clRow.OldValues["Remarks"]?.ToString() ?? "" : "";
                        doc.Add(InlineDiff(old, remarks).SetMarginLeft(20).SetMarginTop(3));
                    }
                }
            }
        }

        // Expiration & Tax Terms table
        private void WriteExpTable(Document doc, List<RowDiff> rows, string mode)
        {
            bool isMod = mode == "mod", isDel = mode == "del";
            float[] cols = isMod
                ? new float[] { 10, 18, 15, 12, 17, 15, 13 }
                : new float[] { 20, 17, 15, 18, 15, 15 };
            var tbl = new Table(UnitValue.CreatePercentArray(cols))
                .UseAllAvailableWidth().SetMarginTop(3).SetMarginLeft(10).SetFontSize(9);

            if (isMod) tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Changed", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Based On", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Terms (y-m)", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("For", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T(isMod ? "" : "Effective For", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("from", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("to", _b, 9))).SetBorder(Border.NO_BORDER));

            foreach (var row in rows.OrderBy(r => G(r, "Type")))
            {
                if (isMod)
                {
                    // To row — new values
                    tbl.AddCell(NB(P(9).SetFont(_b).Add(T("To", _b, 9))));
                    NBCell(tbl, G(row, "Type"));
                    MC(tbl, row, "BasedOn", noBorder: true);
                    NBCell(tbl, $"{G(row, "Yr")} {G(row, "Mo")}");
                    MC(tbl, row, "EffBasedOn", noBorder: true);
                    MC(tbl, row, "EffStartDate", noBorder: true, dateFmt: true);
                    MC(tbl, row, "EffEndDate", noBorder: true, dateFmt: true);
                    // From row — old values
                    tbl.AddCell(NB(P(9).SetFont(_b).Add(T("From", _b, 9))));
                    var ov = row.OldValues ?? new Dictionary<string, object?>();
                    NBCell(tbl, Val(ov, "Type"));
                    NBCell(tbl, Val(ov, "BasedOn"));
                    NBCell(tbl, $"{Val(ov, "Yr")} {Val(ov, "Mo")}");
                    NBCell(tbl, Val(ov, "EffBasedOn"));
                    NBCell(tbl, FmtD(Val(ov, "EffStartDate")));
                    NBCell(tbl, FmtD(Val(ov, "EffEndDate")));
                }
                else
                {
                    var typ = G(row, "Type");
                    var bon = G(row, "BasedOn");
                    var terms = $"{G(row, "Yr")} {G(row, "Mo")}";
                    var eff = G(row, "EffBasedOn");
                    var from = FD(row, "EffStartDate");
                    var to = FD(row, "EffEndDate");
                    if (isDel)
                    {
                        NBCellDel(tbl, typ); NBCellDel(tbl, bon); NBCellDel(tbl, terms);
                        NBCellDel(tbl, eff); NBCellDel(tbl, from); NBCellDel(tbl, to);
                    }
                    else
                    {
                        NBCellY(tbl, typ); NBCellY(tbl, bon); NBCellY(tbl, terms);
                        NBCellY(tbl, eff); NBCellY(tbl, from); NBCellY(tbl, to);
                    }
                }
            }
            doc.Add(tbl);
        }

        // Trademark Law Actions — combined table
        private void WriteTrademarkLawActions(Document doc, List<RowDiff> rows, bool isNewBlock)
        {
            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 26, 13, 5, 5, 5, 16, 14, 16 }))
                .UseAllAvailableWidth().SetMarginTop(3).SetFontSize(9);
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Action Due/Indicator", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Based On", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell(1, 3).Add(P(9).SetFont(_b).SetTextAlignment(TextAlignment.CENTER).Add(T("Terms (y-m-d)", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Effective For", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("From", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("To", _b, 9))).SetBorder(Border.NO_BORDER));

            foreach (var row in rows.OrderBy(r => G(r, "ActionDue")))
            {
                bool added = row.OldValues == null;
                var adInd = $"{G(row, "ActionDue")} ({G(row, "Indicator")})";
                if (added || isNewBlock)
                {
                    NBCellY(tbl, adInd);
                    NBCellY(tbl, G(row, "BasedOn"));
                    NBCellY(tbl, G(row, "Yr")); NBCellY(tbl, G(row, "Mo")); NBCellY(tbl, G(row, "Dy"));
                    NBCellY(tbl, G(row, "EffBasedOn"));
                    NBCellY(tbl, FD(row, "EffStartDate"));
                    NBCellY(tbl, FD(row, "EffEndDate"));
                }
                else
                {
                    // Show old row as being phased out (highlight end-date), new row highlighted fully
                    var ov = row.OldValues!;
                    // Old/terminated row
                    NB2(tbl, $"{Val(ov, "ActionDue")} ({Val(ov, "Indicator")})", false);
                    NB2(tbl, Val(ov, "BasedOn"), false);
                    NB2(tbl, Val(ov, "Yr"), false); NB2(tbl, Val(ov, "Mo"), false); NB2(tbl, Val(ov, "Dy"), false);
                    NB2(tbl, Val(ov, "EffBasedOn"), false);
                    NB2(tbl, FmtD(Val(ov, "EffStartDate")), false);
                    // terminal date gets highlighted to show action was ended
                    NB2(tbl, FD(row, "EffEndDate"), true);

                    // New row — all highlighted
                    NBCellY(tbl, adInd);
                    NBCellY(tbl, G(row, "BasedOn"));
                    NBCellY(tbl, G(row, "Yr")); NBCellY(tbl, G(row, "Mo")); NBCellY(tbl, G(row, "Dy"));
                    NBCellY(tbl, G(row, "EffBasedOn"));
                    NBCellY(tbl, FD(row, "EffStartDate"));
                    NBCellY(tbl, FD(row, "EffEndDate"));
                }
            }
            doc.Add(tbl);
        }

        // Patent Law Actions — "Action:" label before each group
        private void WritePatentLawActions(Document doc, List<RowDiff> rows, bool isNewBlock)
        {
            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 26, 13, 5, 5, 5, 16, 14, 16 }))
                .UseAllAvailableWidth().SetMarginTop(3).SetFontSize(9);
            tbl.AddHeaderCell(new Cell().Add(P(9).Add(T("", _r, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Based On", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell(1, 3).Add(P(9).SetFont(_b).SetTextAlignment(TextAlignment.CENTER).Add(T("Terms (y-m-d)", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Effective for", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("from", _b, 9))).SetBorder(Border.NO_BORDER));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("to", _b, 9))).SetBorder(Border.NO_BORDER));

            foreach (var grp in rows.GroupBy(r => G(r, "ActionDue")).OrderBy(g => g.Key))
            {
                // "Action:" label row
                tbl.AddCell(new Cell(1, 8).Add(P(9).SetFont(_b).SetUnderline().Add(T("Action:", _b, 9)))
                    .SetBorder(Border.NO_BORDER).SetPaddingTop(3));

                foreach (var row in grp)
                {
                    bool added = row.OldValues == null;
                    var name = $"{G(row, "ActionDue")} ({G(row, "Indicator")})";
                    if (added || isNewBlock)
                    {
                        NBCellY(tbl, name); NBCellY(tbl, G(row, "BasedOn"));
                        NBCellY(tbl, G(row, "Yr")); NBCellY(tbl, G(row, "Mo")); NBCellY(tbl, G(row, "Dy"));
                        NBCellY(tbl, G(row, "EffBasedOn"));
                        NBCellY(tbl, FD(row, "EffStartDate"));
                        NBCellY(tbl, FD(row, "EffEndDate"));
                    }
                    else
                    {
                        var ov = row.OldValues!;
                        // Old (terminated) row
                        NB2(tbl, $"{Val(ov, "ActionDue")} ({Val(ov, "Indicator")})", false);
                        NB2(tbl, Val(ov, "BasedOn"), false);
                        NB2(tbl, Val(ov, "Yr"), false); NB2(tbl, Val(ov, "Mo"), false); NB2(tbl, Val(ov, "Dy"), false);
                        NB2(tbl, Val(ov, "EffBasedOn"), false);
                        NB2(tbl, FmtD(Val(ov, "EffStartDate")), false);
                        NB2(tbl, FD(row, "EffEndDate"), true);

                        // New row
                        NBCellY(tbl, name); NBCellY(tbl, G(row, "BasedOn"));
                        NBCellY(tbl, G(row, "Yr")); NBCellY(tbl, G(row, "Mo")); NBCellY(tbl, G(row, "Dy"));
                        NBCellY(tbl, G(row, "EffBasedOn"));
                        NBCellY(tbl, FD(row, "EffStartDate"));
                        NBCellY(tbl, FD(row, "EffEndDate"));
                    }
                }
            }
            doc.Add(tbl);
        }

        // ═══════════════════════════════════════════════════════════════
        // COUNTRY LAW DELETED
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountryLawDeleted(Document doc, MdbComparisonResult comp, string clT)
        {
            if (!H(comp, clT) || !comp.TableDiffs[clT].DeletedRows.Any()) return;
            doc.Add(P(12).SetFont(_b).SetUnderline().SetMarginTop(16).Add(T("Country Law Deleted", _b, 12)));
            foreach (var r in comp.TableDiffs[clT].DeletedRows
                .OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")))
            {
                doc.Add(P(10).SetMarginLeft(10)
                    .Add(T($"Country: {CN(G(r, "Country"))} ({G(r, "Country")})     {G(r, "CaseType")} - {CD(G(r, "CaseType"))}", _r, 10)));
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // INLINE REMARKS DIFF — line-level, whole-line highlight for new
        // or edited lines. A new line matches an old line by normalized
        // whitespace comparison (exact match after trimming/collapsing
        // spaces), so minor reflow doesn't trigger false highlights.
        // ═══════════════════════════════════════════════════════════════
        private Paragraph InlineDiff(string oldText, string newText)
        {
            var p = P(9);
            var oldLines = SplitLines(oldText);
            var newLines = SplitLines(newText);
            var oldSet = new HashSet<string>(oldLines.Select(NormalizeForCompare));

            for (int i = 0; i < newLines.Count; i++)
            {
                var line = newLines[i];
                if (i > 0) p.Add(T("\n", _r, 9));
                var norm = NormalizeForCompare(line);
                if (string.IsNullOrWhiteSpace(norm) || oldSet.Contains(norm))
                    p.Add(T(line, _r, 9));
                else
                    p.Add(T(line, _r, 9).SetBackgroundColor(Yellow));
            }
            return p;
        }

        private static List<string> SplitLines(string? s) =>
            (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n")
                .Split('\n').Select(l => l.TrimEnd()).ToList();

        private static string NormalizeForCompare(string s) =>
            System.Text.RegularExpressions.Regex.Replace((s ?? "").Trim(), @"\s+", " ");

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════
        private static string G(RowDiff r, string c) =>
            r.Values.ContainsKey(c) ? r.Values[c]?.ToString() ?? "" : "";
        private static string Val(Dictionary<string, object?> d, string c) =>
            d.ContainsKey(c) ? d[c]?.ToString() ?? "" : "";
        private string CN(string c) => _cn.ContainsKey(c) ? _cn[c] : c;
        private string CD(string c) => _ctd.ContainsKey(c) ? _ctd[c] : c;
        private static string FD(RowDiff r, string c)
        {
            var v = G(r, c);
            return DateTime.TryParse(v, out var d) ? d.ToString("M/d/yyyy") : v;
        }
        private static string FmtD(string v) =>
            DateTime.TryParse(v, out var d) ? d.ToString("M/d/yyyy") : v;
        private static bool H(MdbComparisonResult c, string t) => c.TableDiffs.ContainsKey(t);

        private Paragraph P(float sz) => new Paragraph().SetFont(_r).SetFontSize(sz);
        private Text T(string s, PdfFont f, float sz) => new Text(s ?? "").SetFont(f).SetFontSize(sz);
        private void HL(Document d, float w) =>
            d.Add(new Paragraph("").SetBorderBottom(new SolidBorder(ColorConstants.LIGHT_GRAY, w))
                .SetMarginTop(4).SetMarginBottom(4));

        // Plain header cell with gray background
        private Cell HC(string t) =>
            new Cell().Add(P(8).SetFont(_b).Add(T(t, _b, 8)))
                .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(3);

        // Plain (bordered) cell
        private void PC(Table t, string v) =>
            t.AddCell(new Cell().Add(P(8).Add(T(v ?? "", _r, 8)))
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2));

        // Yellow bordered cell (new item)
        private void YC(Table t, string v) =>
            t.AddCell(new Cell().Add(P(8).Add(T(v ?? "", _r, 8)).SetBackgroundColor(Yellow))
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2));

        // Deleted (red strike-through style) bordered cell
        private void DC(Table t, string v) =>
            t.AddCell(new Cell().Add(P(8).Add(T(v ?? "", _r, 8).SetLineThrough().SetFontColor(Red)))
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2));

        // Bordered cell — yellow if column changed, else plain
        private void MC(Table t, RowDiff row, string col)
        {
            var v = G(row, col);
            if (row.ChangedColumns.Contains(col)) YC(t, v); else PC(t, v);
        }

        // No-border cell (used in the Expiration / Law Actions tables)
        private Cell NB(Paragraph p) => new Cell().Add(p).SetBorder(Border.NO_BORDER).SetPadding(2);
        private void NBCell(Table t, string v) =>
            t.AddCell(new Cell().Add(P(9).Add(T(v ?? "", _r, 9))).SetBorder(Border.NO_BORDER).SetPadding(2));
        private void NBCellY(Table t, string v) =>
            t.AddCell(new Cell().Add(P(9).Add(T(v ?? "", _r, 9)).SetBackgroundColor(Yellow))
                .SetBorder(Border.NO_BORDER).SetPadding(2));
        private void NBCellDel(Table t, string v) =>
            t.AddCell(new Cell().Add(P(9).Add(T(v ?? "", _r, 9).SetLineThrough().SetFontColor(Red)))
                .SetBorder(Border.NO_BORDER).SetPadding(2));
        private void NB2(Table t, string v, bool highlight)
        {
            var c = new Cell().Add(P(9).Add(T(v ?? "", _r, 9))).SetBorder(Border.NO_BORDER).SetPadding(2);
            if (highlight) c.SetBackgroundColor(Yellow);
            t.AddCell(c);
        }

        // Modified cell with optional no-border and date formatting
        private void MC(Table t, RowDiff row, string col, bool noBorder = false, bool dateFmt = false)
        {
            var v = dateFmt ? FD(row, col) : G(row, col);
            bool changed = row.ChangedColumns.Contains(col);
            var cell = new Cell().Add(P(9).Add(T(v ?? "", _r, 9)));
            if (changed) cell.SetBackgroundColor(Yellow);
            if (noBorder) cell.SetBorder(Border.NO_BORDER).SetPadding(2);
            else cell.SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
            t.AddCell(cell);
        }
    }
}
