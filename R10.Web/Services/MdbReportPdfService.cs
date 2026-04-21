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
            Dictionary<string, string>? cn = null, Dictionary<string, string>? ctd = null,
            string? reportNotes = null)
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
            var atT = $"{pfx}ActionType";
            var paramT = $"{pfx}ActionParameter";
            var expT = comp.IsPatent ? "tblPatCountryExp" : null;
            var expDelT = comp.IsPatent ? "tblPatCountryExpDelete" : null;

            WriteTitle(doc, year, qtr);
            WriteReportNotes(doc, reportNotes);

            // Manual Updates (Action Type diffs) always come first — for both patent
            // and trademark. Patent stops there until country law; trademark follows
            // with Standard Goods (green underline, no yellow highlight — these are
            // manual reference-data updates) and then structural changes.
            WriteManualUpdates(doc, comp, atT, dueT, paramT);
            if (!comp.IsPatent)
            {
                WriteStandardGoods(doc, comp);
                WriteStructural(doc, comp, pfx);
            }

            WriteCountryLawAddedModified(doc, comp, year, qtr, clT, dueT, expT, expDelT);

            if (comp.IsPatent)
                WriteStructural(doc, comp, pfx);

            WriteCountryLawDeleted(doc, comp, clT);

            // Orphan law-action changes (CountryDue changes with no remarks/terms change
            // on the same Country+CaseType) — compact summary at the very end.
            WriteOrphanLawActions(doc, comp, clT, dueT, expT, expDelT);

            doc.Close();
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════
        // REPORT NOTES — author-time free-form text, rendered at top
        // ═══════════════════════════════════════════════════════════════
        private void WriteReportNotes(Document doc, string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return;
            var box = new Paragraph()
                .SetMarginTop(10)
                .SetPadding(8)
                .SetBorder(new SolidBorder(ColorConstants.GRAY, 0.5f))
                .SetBackgroundColor(new DeviceRgb(252, 252, 240));
            foreach (var line in notes.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
            {
                box.Add(T(line, _r, 10));
                box.Add(T("\n", _r, 10));
            }
            doc.Add(box);
        }

        // ═══════════════════════════════════════════════════════════════
        // STANDARD GOODS — trademark only. Renders at the very top of the
        // report (after the title / notes, before Manual Updates). Unlike
        // the rest of the report this uses GREEN UNDERLINED text for new /
        // modified / deleted entries instead of yellow highlight — these are
        // reference-data updates the user applies manually.
        // ═══════════════════════════════════════════════════════════════
        private static readonly DeviceRgb Green = new(0, 130, 0);

        private void WriteStandardGoods(Document doc, MdbComparisonResult comp)
        {
            const string sgT = "tblTmkStandardGood";
            if (!H(comp, sgT)) return;
            var diff = comp.TableDiffs[sgT];
            if (!diff.AddedRows.Any() && !diff.ModifiedRows.Any() && !diff.DeletedRows.Any()) return;

            doc.Add(new AreaBreak());
            doc.Add(P(16).SetFont(_r).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)
                .Add(T("Nice Classification – Standard Goods Update", _r, 16)));

            doc.Add(P(10).SetMarginTop(10)
                .Add(T("Records in the Standard Goods table under the Auxiliary Menu in your system are not added automatically. They will need to be adjusted by a person responsible for data entry.", _r, 10)));

            // Numbered steps
            doc.Add(P(10).SetMarginTop(8).SetMarginLeft(20)
                .Add(T("1. Under the ", _r, 10)).Add(T("Auxiliary", _b, 10)).Add(T(" menu, select ", _r, 10)).Add(T("Standard Goods", _b, 10)));
            doc.Add(P(10).SetMarginLeft(20)
                .Add(T("2. Search and select each of the Classes below.", _r, 10)));
            doc.Add(P(10).SetMarginLeft(20)
                .Add(T("3. Click Edit, then paste the new description into the ", _r, 10)).Add(T("Goods", _b, 10)).Add(T(" field.", _r, 10)));
            doc.Add(P(10).SetMarginLeft(20)
                .Add(T("4. Click ", _r, 10)).Add(T("Save.", _b, 10)));

            // Two-column table: Class | Standard Goods
            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 10, 90 }))
                .UseAllAvailableWidth().SetMarginTop(10).SetFontSize(9);
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Class", _b, 9)))
                .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(4));
            tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T("Standard Goods", _b, 9)))
                .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(4));

            // Union of all changed rows, sorted by class code
            var rows = diff.AddedRows.Select(r => (row: r, mode: "new"))
                .Concat(diff.ModifiedRows.Select(r => (row: r, mode: "mod")))
                .Concat(diff.DeletedRows.Select(r => (row: r, mode: "del")))
                .OrderBy(x => G(x.row, "Class")).ThenBy(x => G(x.row, "ClassType"))
                .ToList();

            foreach (var (row, mode) in rows)
            {
                var cls = G(row, "Class");
                var goods = G(row, "StandardGoods");

                // Class cell
                var clsCell = new Cell().Add(P(9).Add(StyleChange(cls, mode, _r, 9)))
                    .SetBorder(new SolidBorder(0.5f)).SetPadding(4).SetVerticalAlignment(VerticalAlignment.TOP);
                tbl.AddCell(clsCell);

                // Goods cell
                var goodsP = P(9);
                if (mode == "new" || mode == "del")
                {
                    goodsP.Add(StyleChange(goods, mode, _r, 9));
                }
                else if (row.OldValues != null && row.ChangedColumns.Contains("StandardGoods"))
                {
                    var old = row.OldValues.ContainsKey("StandardGoods") ? row.OldValues["StandardGoods"]?.ToString() ?? "" : "";
                    AppendGreenLineDiff(goodsP, old, goods);
                }
                else
                {
                    goodsP.Add(T(goods, _r, 9));
                }
                var goodsCell = new Cell().Add(goodsP)
                    .SetBorder(new SolidBorder(0.5f)).SetPadding(4).SetVerticalAlignment(VerticalAlignment.TOP);
                tbl.AddCell(goodsCell);
            }

            doc.Add(tbl);
        }

        // Style a Text run to reflect Standard-Goods change modes: green+underlined
        // for new/modified, red strikethrough for deleted.
        private Text StyleChange(string s, string mode, PdfFont font, float size)
        {
            var t = T(s, font, size);
            switch (mode)
            {
                case "new":
                case "mod":
                    t.SetFontColor(Green).SetUnderline();
                    break;
                case "del":
                    t.SetFontColor(Red).SetLineThrough();
                    break;
            }
            return t;
        }

        // Like InlineDiff but renders new / changed lines in green+underline
        // instead of yellow background.
        private void AppendGreenLineDiff(Paragraph p, string oldText, string newText)
        {
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
                    p.Add(T(line, _r, 9).SetFontColor(Green).SetUnderline());
            }
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
        // ═══════════════════════════════════════════════════════════════
        // MANUAL UPDATES — driven by the ActionType table diff (tblPatActionType /
        // tblTmkActionType). Each changed Action Type gets a block showing its
        // metadata (name, Office-Action flag, country, follow-up, remarks) plus
        // any matching CountryDue rows that changed for that same Action Type.
        // ═══════════════════════════════════════════════════════════════
        private void WriteManualUpdates(Document doc, MdbComparisonResult comp, string atT, string dueT, string paramT)
        {
            if (!H(comp, atT)) return;
            var diff = comp.TableDiffs[atT];
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
                foreach (var at in diff.AddedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(doc, at, comp, dueT, paramT, "new");
            }
            if (diff.ModifiedRows.Any())
            {
                doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T("Modified Actions", _b, 11)));
                foreach (var at in diff.ModifiedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(doc, at, comp, dueT, paramT, "mod");
            }
            if (diff.DeletedRows.Any())
            {
                doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T("Deleted Actions:", _b, 11)));
                foreach (var at in diff.DeletedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(doc, at, comp, dueT, paramT, "del");
            }
        }

        // Render a single Action Type block under Manual Updates.
        private void WriteActionTypeBlock(Document doc, RowDiff at, MdbComparisonResult comp, string dueT, string paramT, string mode)
        {
            bool isNew = mode == "new", isDel = mode == "del";
            var name = G(at, "ActionType");
            var country = G(at, "Country");
            var isOA = G(at, "IsOfficeAction");
            var oaText = (isOA == "True" || isOA == "true" || isOA == "1" || isOA == "-1") ? "Yes" : "No";

            // Header line: Action Type: NAME          Office Action: Yes/No
            var ap = P(10).SetMarginLeft(30).SetMarginTop(10);
            ap.Add(T("Action Type: ", _r, 10));
            if (isDel) ap.Add(T($"{name} - DELETE", _b, 10).SetBackgroundColor(Red).SetFontColor(ColorConstants.WHITE));
            else ap.Add(T(name, _b, 10).SetBackgroundColor(isNew ? Yellow : (Color?)null));
            ap.Add(T($"          Office Action: {oaText}", _r, 10));
            doc.Add(ap);

            // Country line — rendered as "Name (Code)" to match the rest of the report.
            if (!string.IsNullOrEmpty(country))
            {
                var cp = P(10).SetMarginLeft(50).SetMarginTop(2);
                cp.Add(T("Country: ", _r, 10));
                var label = $"{CN(country)} ({country})";
                if (isNew) cp.Add(T(label, _r, 10).SetBackgroundColor(Yellow));
                else cp.Add(T(label, _r, 10));
                doc.Add(cp);
            }

            // CountryDue rows are intentionally NOT listed here. Manual Updates
            // focuses on the Action Type metadata itself (header, remarks, follow-up).
            // Per-country due-date changes appear under Country Law > Law Actions.

            // Remarks (with inline line-diff for modified action types)
            var remarks = G(at, "Remarks");
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                var rt = new Table(UnitValue.CreatePercentArray(new float[] { 12, 88 }))
                    .UseAllAvailableWidth().SetMarginLeft(50).SetMarginTop(6);
                rt.AddCell(new Cell().Add(P(9).Add(T("Remarks:", _r, 9)))
                    .SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetPaddingRight(4));
                var remCell = new Cell().SetBorder(Border.NO_BORDER);
                if (isNew)
                    remCell.Add(P(9).Add(T(remarks, _r, 9).SetBackgroundColor(Yellow)));
                else if (isDel)
                    remCell.Add(P(9).Add(T(remarks, _r, 9).SetLineThrough()));
                else if (at.OldValues != null && at.ChangedColumns.Contains("Remarks"))
                {
                    var old = at.OldValues.ContainsKey("Remarks") ? at.OldValues["Remarks"]?.ToString() ?? "" : "";
                    remCell.Add(InlineDiff(old, remarks));
                }
                else
                    remCell.Add(P(9).Add(T(remarks, _r, 9)));
                rt.AddCell(remCell);
                doc.Add(rt);
            }

            // Follow-up info from the ActionType record (not CountryDue)
            var fuMsg = G(at, "FollowUpMsg");
            var fuMo = G(at, "FollowUpMonth");
            var fuDy = G(at, "FollowUpDay");
            var fuGen = G(at, "FollowUpGen");
            if (!string.IsNullOrWhiteSpace(fuMsg) || !string.IsNullOrEmpty(fuMo) || !string.IsNullOrEmpty(fuGen))
            {
                if (!string.IsNullOrWhiteSpace(fuMsg))
                    doc.Add(P(9).SetMarginLeft(50).SetMarginTop(4)
                        .Add(T($"Follow Up Action: {fuMsg}", _r, 9)));
                doc.Add(P(9).SetMarginLeft(50)
                    .Add(T($"Follow Up Term: {fuMo} Month(s) / {fuDy} Day(s)", _r, 9)));
                doc.Add(P(9).SetMarginLeft(50)
                    .Add(T($"Follow Up Based On: {FollowUpGenLabel(fuGen)}", _r, 9)));
            }

            // Action Parameter diffs for THIS action type (matched by ActionTypeID).
            // Orphan parameter changes (parent ActionType unchanged) are never shown —
            // they simply don't appear in the report.
            WriteActionParameterSubTable(doc, at, comp, paramT, mode);

            HL(doc, 0.3f);
        }

        // Sub-table of Action Parameter changes scoped to a single Action Type.
        // Columns: Action Due | Yr | Mo | Dy | Indicator | Change.
        // - Added params: full-row yellow.
        // - Modified params: per-cell yellow on changed columns only.
        // - Deleted params: red strikethrough.
        // For added/deleted parent Action Types, every matching param is treated as
        // added/deleted respectively (even if the comparison classed them differently
        // due to ActionTypeID identity shifts).
        private void WriteActionParameterSubTable(Document doc, RowDiff at, MdbComparisonResult comp,
            string paramT, string parentMode)
        {
            if (!H(comp, paramT)) return;
            var atId = G(at, "ActionTypeID");
            if (string.IsNullOrEmpty(atId)) return;

            var pd = comp.TableDiffs[paramT];
            bool Matches(RowDiff r) => G(r, "ActionTypeID") == atId;

            var added = pd.AddedRows.Where(Matches).ToList();
            var modified = pd.ModifiedRows.Where(Matches).ToList();
            var deleted = pd.DeletedRows.Where(Matches).ToList();
            if (!added.Any() && !modified.Any() && !deleted.Any()) return;

            doc.Add(P(9).SetFont(_b).SetMarginLeft(50).SetMarginTop(6)
                .Add(T("Action Parameters", _b, 9)));

            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 35, 10, 10, 10, 20, 15 }))
                .UseAllAvailableWidth().SetMarginLeft(50).SetMarginTop(3).SetFontSize(8);
            foreach (var h in new[] { "Action Due", "Yr", "Mo", "Dy", "Indicator", "Change" })
                tbl.AddHeaderCell(new Cell().Add(P(8).SetFont(_b).Add(T(h, _b, 8)))
                    .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(2));

            // mode: "add" = whole row yellow, "mod" = per-cell yellow, "del" = strikethrough.
            void Row(RowDiff r, string tag, string rowMode)
            {
                void C(string v, bool highlight, bool strike = false)
                {
                    var text = T(v ?? "", _r, 8);
                    if (strike) text.SetLineThrough().SetFontColor(Red);
                    var cell = new Cell().Add(P(8).Add(text))
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
                    if (highlight) cell.SetBackgroundColor(Yellow);
                    tbl.AddCell(cell);
                }
                bool allYellow = rowMode == "add";
                bool allStrike = rowMode == "del";
                bool Ch(string col) =>
                    allYellow || (rowMode == "mod" && r.ChangedColumns.Contains(col));

                C(G(r, "ActionDue"), Ch("ActionDue"), allStrike);
                C(G(r, "Yr"), Ch("Yr"), allStrike);
                C(G(r, "Mo"), Ch("Mo"), allStrike);
                C(G(r, "Dy"), Ch("Dy"), allStrike);
                C(G(r, "Indicator"), Ch("Indicator"), allStrike);
                C(tag, allYellow || rowMode == "mod", false);
            }

            // If the parent ActionType is new, every matching parameter is by definition
            // part of the new action — render them all as added even if the comparison
            // considers them modified/etc.
            string EffectiveMode(RowDiff r)
            {
                if (parentMode == "new") return "add";
                if (parentMode == "del") return "del";
                return r.OldValues == null ? "add" : "mod";
            }

            foreach (var r in added.OrderBy(r => G(r, "ActionDue")))
                Row(r, "Added", EffectiveMode(r));
            foreach (var r in modified.OrderBy(r => G(r, "ActionDue")))
                Row(r, "Modified", EffectiveMode(r));
            foreach (var r in deleted.OrderBy(r => G(r, "ActionDue")))
                Row(r, "Deleted", parentMode == "new" ? "add" : "del");

            doc.Add(tbl);
        }

        // Action Due table (shared by Manual Updates + Country Law).
        // Columns: [Country?] | Action Due | Yr | Mo | Dy | Indicator |
        //          Based On | Effective From | To | Effective Based On
        // - Whole row yellow for brand-new rows (or when treatAllAsNew is set
        //   because the country-law block itself is new).
        // - Per-cell yellow on modified rows (only changed columns highlight).
        // - Red strikethrough for deleted rows.
        private void WriteActionDueTable(Document doc, List<RowDiff> rows,
            bool treatAllAsNew = false, bool treatAllAsDeleted = false, float marginLeft = 20,
            bool includeCountry = false)
        {
            float[] cols = includeCountry
                ? new float[] { 14, 18, 5, 5, 5, 10, 10, 11, 11, 11 }
                : new float[] { 20, 5, 5, 5, 11, 11, 13, 13, 17 };
            var tbl = new Table(UnitValue.CreatePercentArray(cols))
                .UseAllAvailableWidth().SetMarginLeft(marginLeft).SetMarginTop(6).SetFontSize(9);
            if (includeCountry) tbl.AddHeaderCell(HC("Country"));
            tbl.AddHeaderCell(HC("Action Due"));
            tbl.AddHeaderCell(HC("Yr"));
            tbl.AddHeaderCell(HC("Mo"));
            tbl.AddHeaderCell(HC("Dy"));
            tbl.AddHeaderCell(HC("Indicator"));
            tbl.AddHeaderCell(HC("Based On"));
            tbl.AddHeaderCell(HC("Effective From"));
            tbl.AddHeaderCell(HC("To"));
            tbl.AddHeaderCell(HC("Effective Based On"));

            foreach (var row in rows
                .OrderBy(r => includeCountry ? CN(G(r, "Country")) : "")
                .ThenBy(r => G(r, "ActionDue")))
            {
                bool isAdd = row.OldValues == null;
                string countryLabel = includeCountry
                    ? $"{CN(G(row, "Country"))} ({G(row, "Country")})"
                    : "";
                string effFrom = FD(row, "EffStartDate");
                string effTo = FD(row, "EffEndDate");
                if (treatAllAsDeleted)
                {
                    if (includeCountry) DC(tbl, countryLabel);
                    DC(tbl, G(row, "ActionDue")); DC(tbl, G(row, "Yr")); DC(tbl, G(row, "Mo"));
                    DC(tbl, G(row, "Dy")); DC(tbl, G(row, "Indicator"));
                    DC(tbl, G(row, "BasedOn")); DC(tbl, effFrom); DC(tbl, effTo); DC(tbl, G(row, "EffBasedOn"));
                }
                else if (treatAllAsNew || isAdd)
                {
                    if (includeCountry) YC(tbl, countryLabel);
                    YC(tbl, G(row, "ActionDue")); YC(tbl, G(row, "Yr")); YC(tbl, G(row, "Mo"));
                    YC(tbl, G(row, "Dy")); YC(tbl, G(row, "Indicator"));
                    YC(tbl, G(row, "BasedOn")); YC(tbl, effFrom); YC(tbl, effTo); YC(tbl, G(row, "EffBasedOn"));
                }
                else
                {
                    if (includeCountry) PC(tbl, countryLabel);
                    MC(tbl, row, "ActionDue"); MC(tbl, row, "Yr"); MC(tbl, row, "Mo");
                    MC(tbl, row, "Dy"); MC(tbl, row, "Indicator");
                    MC(tbl, row, "BasedOn");
                    MCDate(tbl, row, "EffStartDate");
                    MCDate(tbl, row, "EffEndDate");
                    MC(tbl, row, "EffBasedOn");
                }
            }
            doc.Add(tbl);
        }

        // Bordered MC variant that formats the value as a date (M/d/yyyy).
        private void MCDate(Table t, RowDiff row, string col)
        {
            var v = FD(row, col);
            if (row.ChangedColumns.Contains(col)) YC(t, v); else PC(t, v);
        }

        private static string FollowUpGenLabel(string v) => v switch
        {
            "0" => "Don't Generate",
            "1" => "Response Sent Date",
            "2" => "Reminder Date",
            _ => string.IsNullOrEmpty(v) ? "" : v
        };

        // ═══════════════════════════════════════════════════════════════
        // TRADEMARK STRUCTURAL: Areas, Case Types, Designations
        // ═══════════════════════════════════════════════════════════════
        private void WriteStructural(Document doc, MdbComparisonResult c, string pfx)
        {
            var ctTbl = $"{pfx}CaseType";
            var desTbl = $"{pfx}DesCaseType";
            var desDelTbl = $"{pfx}DesCaseTypeDelete";

            // Area Countries changes are intentionally NOT rendered — the Area/Country
            // tables are still exported + diffed so history is preserved between
            // releases, but they don't appear in the PDF.

            // Case Types Added — yellow highlight. Force onto a new page so a long
            // Standard Goods list (trademark) doesn't bleed into the case-type lists.
            if (H(c, ctTbl) && c.TableDiffs[ctTbl].AddedRows.Any())
            {
                doc.Add(new AreaBreak());
                SectionHeader(doc, "Case Types Added");
                foreach (var r in c.TableDiffs[ctTbl].AddedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "CaseType"), _r, 9).SetBackgroundColor(Yellow))).SetBorder(Border.NO_BORDER).SetPaddingLeft(15));
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "Description"), _r, 9).SetBackgroundColor(Yellow))).SetBorder(Border.NO_BORDER));
                    doc.Add(t);
                }
            }

            // Case Types Deleted — red strikethrough.
            if (H(c, ctTbl) && c.TableDiffs[ctTbl].DeletedRows.Any())
            {
                SectionHeader(doc, "Case Types Deleted");
                foreach (var r in c.TableDiffs[ctTbl].DeletedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "CaseType"), _r, 9).SetLineThrough().SetFontColor(Red))).SetBorder(Border.NO_BORDER).SetPaddingLeft(15));
                    t.AddCell(new Cell().Add(P(9).Add(T(G(r, "Description"), _r, 9).SetLineThrough().SetFontColor(Red))).SetBorder(Border.NO_BORDER));
                    doc.Add(t);
                }
            }

            // Designation Deleted — red strikethrough.
            var desDel = new List<RowDiff>();
            if (H(c, desTbl)) desDel.AddRange(c.TableDiffs[desTbl].DeletedRows);
            if (H(c, desDelTbl)) desDel.AddRange(c.TableDiffs[desDelTbl].AddedRows);
            if (desDel.Any())
            {
                SectionHeader(doc, "Designation Deleted");
                WriteDesignations(doc, desDel, added: false);
            }

            // Designation Added — yellow highlight.
            if (H(c, desTbl) && c.TableDiffs[desTbl].AddedRows.Any())
            {
                SectionHeader(doc, "Designation Added");
                WriteDesignations(doc, c.TableDiffs[desTbl].AddedRows, added: true);
            }
        }

        private void WriteDesignations(Document doc, List<RowDiff> rows, bool added)
        {
            // Added → yellow highlight; Deleted → red strikethrough.
            Text Style(Text t) => added ? t.SetBackgroundColor(Yellow) : t.SetLineThrough().SetFontColor(Red);

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
                        .Add(Style(T($"{CN(ioGrp.Key)} ({ioGrp.Key})", _b, 9)))).SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).SetFont(_b).SetMarginTop(4)
                        .Add(Style(T($"{ct} - {CD(ct)}", _b, 9)))).SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).Add(T(added ? "Can be designated in" : "designated in", _r, 9)))
                        .SetBorder(Border.NO_BORDER));
                    tbl.AddCell(new Cell().Add(P(9).Add(T("as", _r, 9))).SetBorder(Border.NO_BORDER));
                    foreach (var r in ctGrp.OrderBy(r => CN(G(r, "DesCountry"))))
                    {
                        tbl.AddCell(new Cell().Add(P(9).Add(Style(T($"{CN(G(r, "DesCountry"))} ({G(r, "DesCountry")})", _r, 9))))
                            .SetBorder(Border.NO_BORDER));
                        tbl.AddCell(new Cell().Add(P(9).Add(Style(T(G(r, "DesCaseType"), _r, 9))))
                            .SetBorder(Border.NO_BORDER));
                    }
                }
            }
            doc.Add(tbl);
        }

        private void SectionHeader(Document doc, string text) =>
            doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(14).Add(T(text, _b, 11)));

        // ═══════════════════════════════════════════════════════════════
        // COUNTRY LAW ADDED/MODIFIED — one page per country/case-type.
        // Country/case-types whose only change is in CountryDue (no Remarks or
        // Expiration change) are skipped here and rendered in the orphan table
        // at the end of the report (see WriteOrphanLawActions).
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountryLawAddedModified(Document doc, MdbComparisonResult comp, string year, string qtr,
            string clT, string dueT, string? expT, string? expDelT)
        {
            var fullBlockKeys = CountryLawBlockKeys(comp, clT, expT, expDelT);
            if (!fullBlockKeys.Any()) return;

            doc.Add(new AreaBreak());
            WriteTitle(doc, year, qtr);
            doc.Add(P(11).SetFont(_b).SetUnderline().SetMarginTop(6)
                .Add(T("Country Law Added/Modified", _b, 11)));

            var newClKeys = new HashSet<(string, string)>();
            if (H(comp, clT))
                foreach (var r in comp.TableDiffs[clT].AddedRows)
                    newClKeys.Add((G(r, "Country"), G(r, "CaseType")));

            bool first = true;
            foreach (var (country, caseType) in fullBlockKeys.OrderBy(k => CN(k.Item1)).ThenBy(k => k.Item2))
            {
                if (!first) doc.Add(new AreaBreak());
                first = false;
                bool isNewBlock = newClKeys.Contains((country, caseType));
                WriteCountryBlock(doc, comp, country, caseType, isNewBlock, clT, dueT, expT, expDelT);
            }
        }

        // A country/case-type gets a full block if its CountryLaw (remarks) or
        // CountryExp / CountryExpDelete changed. Pure CountryDue changes go to
        // the orphan table.
        private static HashSet<(string, string)> CountryLawBlockKeys(MdbComparisonResult comp,
            string clT, string? expT, string? expDelT)
        {
            var keys = new HashSet<(string, string)>();
            void Add(IEnumerable<RowDiff>? rows)
            {
                if (rows == null) return;
                foreach (var r in rows) keys.Add((G(r, "Country"), G(r, "CaseType")));
            }
            if (H(comp, clT)) { Add(comp.TableDiffs[clT].AddedRows); Add(comp.TableDiffs[clT].ModifiedRows); }
            if (expT != null && H(comp, expT)) { Add(comp.TableDiffs[expT].AddedRows); Add(comp.TableDiffs[expT].ModifiedRows); }
            if (expDelT != null && H(comp, expDelT)) Add(comp.TableDiffs[expDelT].AddedRows);
            return keys;
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

            // Law Actions — adds/mods and deletes, all in the same 5-col tabular
            // format used by Manual Updates.
            if (H(comp, dueT))
            {
                var dd = comp.TableDiffs[dueT];
                bool Matches(RowDiff r) => G(r, "Country") == country && G(r, "CaseType") == caseType;
                var addMod = dd.AddedRows.Where(Matches).Concat(dd.ModifiedRows.Where(Matches)).ToList();
                var deleted = dd.DeletedRows.Where(Matches).ToList();

                if (addMod.Any() || deleted.Any())
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6).Add(T("Law Actions", _b, 10)));
                if (addMod.Any())
                    WriteActionDueTable(doc, addMod, treatAllAsNew: isNewBlock, marginLeft: 10);
                if (deleted.Any())
                {
                    doc.Add(P(9).SetFont(_i).SetMarginLeft(10).SetMarginTop(4)
                        .Add(T("Deleted:", _i, 9)));
                    WriteActionDueTable(doc, deleted, treatAllAsDeleted: true, marginLeft: 10);
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

        // ═══════════════════════════════════════════════════════════════
        // ORPHAN LAW ACTIONS — Country/CaseType pairs whose only change is
        // in CountryDue (no change to CountryLaw remarks or CountryExp).
        // Compact summary at the end of the report.
        // ═══════════════════════════════════════════════════════════════
        private void WriteOrphanLawActions(Document doc, MdbComparisonResult comp,
            string clT, string dueT, string? expT, string? expDelT)
        {
            if (!H(comp, dueT)) return;
            var dd = comp.TableDiffs[dueT];
            var fullBlockKeys = CountryLawBlockKeys(comp, clT, expT, expDelT);

            bool IsOrphan(RowDiff r) => !fullBlockKeys.Contains((G(r, "Country"), G(r, "CaseType")));
            var orphanAdd = dd.AddedRows.Where(IsOrphan).ToList();
            var orphanMod = dd.ModifiedRows.Where(IsOrphan).ToList();
            var orphanDel = dd.DeletedRows.Where(IsOrphan).ToList();
            if (!orphanAdd.Any() && !orphanMod.Any() && !orphanDel.Any()) return;

            doc.Add(new AreaBreak());
            doc.Add(P(12).SetFont(_b).SetUnderline().SetMarginTop(6)
                .Add(T("Other Law Action Changes", _b, 12)));
            doc.Add(P(9).SetMarginTop(4)
                .Add(T("The following law action changes have no corresponding Law Highlights changes.", _r, 9)));

            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 15, 8, 24, 12, 7, 7, 7, 12 }))
                .UseAllAvailableWidth().SetMarginTop(6).SetFontSize(8);
            foreach (var h in new[] { "Country", "Case", "Action Due (Indicator)", "Based On", "Yr", "Mo", "Dy", "Change" })
                tbl.AddHeaderCell(new Cell().Add(P(8).SetFont(_b).Add(T(h, _b, 8)))
                    .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(2));

            // mode: "add" = whole row yellow, "mod" = per-cell yellow on changed
            // columns only, "del" = no highlight (tag cell carries the state).
            void Row(RowDiff r, string tag, string mode)
            {
                void C(string v, bool highlight)
                {
                    var cell = new Cell().Add(P(8).Add(T(v ?? "", _r, 8)))
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
                    if (highlight) cell.SetBackgroundColor(Yellow);
                    tbl.AddCell(cell);
                }
                bool allYellow = mode == "add";
                bool Ch(params string[] cols) =>
                    allYellow || (mode == "mod" && cols.Any(c => r.ChangedColumns.Contains(c)));

                C($"{CN(G(r, "Country"))} ({G(r, "Country")})", allYellow);
                C(G(r, "CaseType"), allYellow);
                C($"{G(r, "ActionDue")} ({G(r, "Indicator")})", Ch("ActionDue", "Indicator"));
                C(G(r, "BasedOn"), Ch("BasedOn"));
                C(G(r, "Yr"), Ch("Yr"));
                C(G(r, "Mo"), Ch("Mo"));
                C(G(r, "Dy"), Ch("Dy"));
                C(tag, allYellow || mode == "mod");
            }

            foreach (var r in orphanAdd.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "Added", "add");
            foreach (var r in orphanMod.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "Modified", "mod");
            foreach (var r in orphanDel.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "Deleted", "del");

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
