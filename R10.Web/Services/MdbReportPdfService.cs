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
        // Whether the page break before the "Other ... Changes" tail sections
        // has already been emitted, so the law-action and expiration orphan
        // tables share one break instead of each forcing their own page.
        private bool _orphanBreakAdded;

        // R8+ reports drop "and Tax" from the expiration-section headers
        // ("Expiration Terms Added" instead of "Expiration and Tax Terms Added").
        // Detected from the release name in GenerateReport.
        private bool _isR8Plus;

        public byte[] GenerateReport(MdbComparisonResult comp, string name, string year, string qtr,
            Dictionary<string, string>? cn = null, Dictionary<string, string>? ctd = null,
            string? reportNotes = null)
        {
            _cn = cn ?? new();
            _ctd = ctd ?? new();
            _orphanBreakAdded = false;
            _isR8Plus = IsR8PlusName(name);

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

            // Order differs by report type:
            //   Patent:    Structural → Manual Updates (legacy patent layout).
            //   Trademark: Standard Goods → Structural. No Manual Updates section —
            //              trademark ActionType changes are handled via the
            //              CountryDue-driven Law Actions tables in each country
            //              block (and the orphan table for bare due-date edits).
            if (comp.IsPatent)
            {
                WriteStructural(doc, comp, pfx);
                WriteManualUpdates(doc, comp, atT, dueT, paramT);
            }
            else
            {
                WriteStandardGoods(doc, comp);
                WriteStructural(doc, comp, pfx);
            }

            WriteCountryLawAddedModified(doc, comp, year, qtr, clT, dueT, expT, expDelT);

            WriteCountryLawDeleted(doc, comp, clT);

            // Orphan changes — Country+CaseType pairs whose only edits are in
            // CountryDue (Law Actions) or CountryExp (Expiration / Tax Terms),
            // with no Remarks change. Each lands in its own compact table at
            // the end of the report. They share a single page break.
            WriteOrphanLawActions(doc, comp, clT, dueT, expT, expDelT);
            WriteOrphanExpirationChanges(doc, comp, clT, expT, expDelT);

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

            // Inline with Manual Updates + structural — no forced page break.
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

        // Like InlineDiff but renders inserted / changed words in green+underline
        // instead of yellow background.
        private void AppendGreenLineDiff(Paragraph p, string oldText, string newText)
        {
            foreach (var seg in WordDiffSegments(oldText, newText))
            {
                var t = T(seg.text, _r, 9);
                if (seg.changed) t.SetFontColor(Green).SetUnderline();
                p.Add(t);
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

            // Order: Action Parameters → Remarks → Follow-Up. Clients care most
            // about the deadline parameters; the prose remarks come after.
            // Action Parameter diffs for THIS action type (matched by ActionTypeID).
            WriteActionParameterSubTable(doc, at, comp, paramT, mode);

            // Remarks (with inline word-diff for modified action types)
            var remarks = G(at, "Remarks");
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                var rt = new Table(UnitValue.CreatePercentArray(new float[] { 12, 88 }))
                    .UseAllAvailableWidth().SetMarginLeft(50).SetMarginTop(6);
                rt.AddCell(new Cell().Add(P(9).Add(T("Remarks:", _r, 9)))
                    .SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetPaddingRight(4));
                var remCell = new Cell().SetBorder(Border.NO_BORDER);
                if (isNew)
                    remCell.Add(P(9).Add(T(remarks, _r, 9).SetBackgroundColor(Yellow).SetUnderline()));
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

            HL(doc, 0.3f);
        }

        // Sub-table of Action Parameter changes scoped to a single Action Type.
        // Columns: Action Due | Yr | Mo | Dy | Indicator.
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

            // Only treat a Modified row as a "real" change if at least one of
            // the columns we actually display (ActionDue/Yr/Mo/Dy/Indicator)
            // appears in ChangedColumns. Rows where only ignored/non-displayed
            // columns shifted would otherwise render as a plain, unhighlighted
            // row — confusing readers into thinking nothing changed.
            var displayCols = new[] { "ActionDue", "Yr", "Mo", "Dy", "Indicator" };
            var added = pd.AddedRows.Where(Matches).ToList();
            var modified = pd.ModifiedRows.Where(Matches)
                .Where(r => displayCols.Any(c => r.ChangedColumns.Contains(c)))
                .ToList();
            var deleted = pd.DeletedRows.Where(Matches).ToList();
            if (!added.Any() && !modified.Any() && !deleted.Any()) return;

            doc.Add(P(9).SetFont(_b).SetMarginLeft(50).SetMarginTop(6)
                .Add(T("Action Parameters", _b, 9)));

            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 40, 12, 12, 12, 24 }))
                .UseAllAvailableWidth().SetMarginLeft(50).SetMarginTop(3).SetFontSize(8);
            foreach (var h in new[] { "Action Due", "Yr", "Mo", "Dy", "Indicator" })
                tbl.AddHeaderCell(new Cell().Add(P(8).SetFont(_b).Add(T(h, _b, 8)))
                    .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(2));

            // mode: "add" = whole row yellow, "mod" = per-cell yellow, "del" = strikethrough.
            void Row(RowDiff r, string rowMode)
            {
                void C(string v, bool highlight, bool strike)
                {
                    var text = T(v ?? "", _r, 8);
                    if (strike) text.SetLineThrough().SetFontColor(Red);
                    var cell = new Cell().Add(P(8).Add(text))
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
                    if (highlight && !strike) cell.SetBackgroundColor(Yellow);
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
                Row(r, EffectiveMode(r));
            foreach (var r in modified.OrderBy(r => G(r, "ActionDue")))
                Row(r, EffectiveMode(r));
            foreach (var r in deleted.OrderBy(r => G(r, "ActionDue")))
                Row(r, parentMode == "new" ? "add" : "del");

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
            var acTbl = $"{pfx}AreaCountry";
            var acDelTbl = $"{pfx}AreaCountryDelete";
            var ctTbl = $"{pfx}CaseType";
            var desTbl = $"{pfx}DesCaseType";
            var desDelTbl = $"{pfx}DesCaseTypeDelete";

            // Area Countries Deleted — red strikethrough. Grouped by Area.
            // A single deletion typically shows up in BOTH the primary table
            // (as DeletedRows) and the companion *Delete tracker (as AddedRows),
            // so we dedupe by the natural key (Area, Country).
            var acDel = new List<RowDiff>();
            if (H(c, acTbl)) acDel.AddRange(c.TableDiffs[acTbl].DeletedRows);
            if (H(c, acDelTbl)) acDel.AddRange(c.TableDiffs[acDelTbl].AddedRows);
            acDel = acDel
                .GroupBy(r => (G(r, "Area"), G(r, "Country")))
                .Select(g => g.First())
                .ToList();
            if (acDel.Any())
            {
                SectionHeader(doc, "Area Countries Deleted");
                foreach (var g in acDel.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    doc.Add(P(10).SetFont(_b).SetMarginTop(4).Add(T(g.Key, _b, 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        doc.Add(P(9).SetMarginLeft(30)
                            .Add(T($"{CN(G(r, "Country"))} ({G(r, "Country")})", _r, 9)
                                .SetLineThrough().SetFontColor(Red)));
                }
            }

            // Area Countries Added — yellow highlight. Grouped by Area.
            if (H(c, acTbl) && c.TableDiffs[acTbl].AddedRows.Any())
            {
                SectionHeader(doc, "Area Countries Added");
                foreach (var g in c.TableDiffs[acTbl].AddedRows.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    doc.Add(P(10).SetFont(_b).SetMarginTop(4).Add(T(g.Key, _b, 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        doc.Add(P(9).SetMarginLeft(30)
                            .Add(T($"{CN(G(r, "Country"))} ({G(r, "Country")})", _r, 9)
                                .SetBackgroundColor(Yellow)));
                }
            }

            // Case Types Added — yellow highlight. Patent forces a page break
            // here because its structural sections (Area Countries, Designations)
            // can run long; trademark keeps Case Types inline with the rest of
            // the structural block.
            if (H(c, ctTbl) && c.TableDiffs[ctTbl].AddedRows.Any())
            {
                if (c.IsPatent) doc.Add(new AreaBreak());
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

            // Designation Deleted — red strikethrough. Same dedupe pattern as
            // Area Countries Deleted: a single removal lands in both the primary
            // DesCaseType table and the DesCaseTypeDelete tracker.
            var desDel = new List<RowDiff>();
            if (H(c, desTbl)) desDel.AddRange(c.TableDiffs[desTbl].DeletedRows);
            if (H(c, desDelTbl)) desDel.AddRange(c.TableDiffs[desDelTbl].AddedRows);
            desDel = desDel
                .GroupBy(r => (G(r, "IntlCode"), G(r, "CaseType"), G(r, "DesCountry"), G(r, "DesCaseType")))
                .Select(g => g.First())
                .ToList();
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

        // A country/case-type gets a full block ONLY if its CountryLaw (Remarks /
        // narrative) row changed. Pure CountryDue changes and pure Expiration /
        // Tax-Term changes go to the orphan tables at the end of the report —
        // a one-line modification doesn't deserve a whole page block.
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
                var hdrPrefix = _isR8Plus ? "Expiration Terms" : "Expiration and Tax Terms";
                if (added.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T($"{hdrPrefix} Added", _b, 10)));
                    WriteExpTable(doc, added, mode: "add");
                }
                if (modified.Any())
                {
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T($"{hdrPrefix} Modified", _b, 10)));
                    WriteExpTable(doc, modified, mode: "mod");
                }
            }
            if (expDelT != null && H(comp, expDelT))
            {
                var del = comp.TableDiffs[expDelT].AddedRows
                    .Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                if (del.Any())
                {
                    var hdrPrefix = _isR8Plus ? "Expiration Terms" : "Expiration and Tax Terms";
                    doc.Add(P(10).SetFont(_b).SetUnderline().SetMarginTop(6)
                        .Add(T($"{hdrPrefix} Deleted", _b, 10)));
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
                        if (isNewBlock) rP.Add(T(remarks, _r, 9).SetBackgroundColor(Yellow).SetUnderline());
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

        // Expiration & Tax Terms table. One row per change. Adds get the whole
        // row highlighted, mods get only the changed cells highlighted, deletes
        // get red strikethrough. The previous To/From two-row layout for mods
        // was retired — yellow on the changed cell tells the same story without
        // the extra visual noise.
        private void WriteExpTable(Document doc, List<RowDiff> rows, string mode)
        {
            bool isMod = mode == "mod", isDel = mode == "del";
            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 20, 17, 15, 18, 15, 15 }))
                .UseAllAvailableWidth().SetMarginTop(3).SetMarginLeft(10).SetFontSize(9);

            foreach (var h in new[] { "Based On", "Terms (y-m)", "For", "Effective For", "from", "to" })
                tbl.AddHeaderCell(new Cell().Add(P(9).SetFont(_b).Add(T(h, _b, 9))).SetBorder(Border.NO_BORDER));

            foreach (var row in rows.OrderBy(r => G(r, "Type")))
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
                else if (isMod)
                {
                    bool Ch(params string[] cols) => cols.Any(c => row.ChangedColumns.Contains(c));
                    void Cell(string v, bool y) { if (y) NBCellY(tbl, v); else NBCell(tbl, v); }
                    Cell(typ, Ch("Type"));
                    Cell(bon, Ch("BasedOn"));
                    Cell(terms, Ch("Yr", "Mo"));
                    Cell(eff, Ch("EffBasedOn"));
                    Cell(from, Ch("EffStartDate"));
                    Cell(to, Ch("EffEndDate"));
                }
                else
                {
                    NBCellY(tbl, typ); NBCellY(tbl, bon); NBCellY(tbl, terms);
                    NBCellY(tbl, eff); NBCellY(tbl, from); NBCellY(tbl, to);
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

            EnsureOrphanBreak(doc);
            doc.Add(P(12).SetFont(_b).SetUnderline().SetMarginTop(6)
                .Add(T("Other Law Action Changes", _b, 12)));
            doc.Add(P(9).SetMarginTop(4)
                .Add(T("The following law action changes have no corresponding Law Highlights changes.", _r, 9)));

            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 17, 9, 27, 14, 8, 8, 8 }))
                .UseAllAvailableWidth().SetMarginTop(6).SetFontSize(8);
            foreach (var h in new[] { "Country", "Case", "Action Due (Indicator)", "Based On", "Yr", "Mo", "Dy" })
                tbl.AddHeaderCell(new Cell().Add(P(8).SetFont(_b).Add(T(h, _b, 8)))
                    .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(2));

            // Identifier cols (Country, Case) are never highlighted. Data cols
            // are yellow on add (whole row new) or on mod when r.ChangedColumns
            // names them. Deletes get red strikethrough — that's enough to read
            // the row's intent without a separate Change column.
            void Row(RowDiff r, string mode)
            {
                bool isAdd = mode == "add", isMod = mode == "mod", isDel = mode == "del";
                bool Ch(params string[] cols) =>
                    isAdd || (isMod && cols.Any(c => r.ChangedColumns.Contains(c)));

                void C(string v, bool highlight)
                {
                    var t = T(v ?? "", _r, 8);
                    if (isDel) t.SetLineThrough().SetFontColor(Red);
                    var cell = new Cell().Add(P(8).Add(t))
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
                    if (highlight && !isDel) cell.SetBackgroundColor(Yellow);
                    tbl.AddCell(cell);
                }

                C($"{CN(G(r, "Country"))} ({G(r, "Country")})", false);
                C(G(r, "CaseType"), false);
                C($"{G(r, "ActionDue")} ({G(r, "Indicator")})", Ch("ActionDue", "Indicator"));
                C(G(r, "BasedOn"), Ch("BasedOn"));
                C(G(r, "Yr"), Ch("Yr"));
                C(G(r, "Mo"), Ch("Mo"));
                C(G(r, "Dy"), Ch("Dy"));
            }

            foreach (var r in orphanAdd.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "add");
            foreach (var r in orphanMod.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "mod");
            foreach (var r in orphanDel.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue")))
                Row(r, "del");

            doc.Add(tbl);
        }

        // Emit the page break that separates the orphan tail tables from the
        // country-block body, but only once per report — so if both this and
        // the expiration orphan section render they share a single break.
        private void EnsureOrphanBreak(Document doc)
        {
            if (_orphanBreakAdded) return;
            doc.Add(new AreaBreak());
            _orphanBreakAdded = true;
        }

        // ═══════════════════════════════════════════════════════════════
        // ORPHAN EXPIRATION CHANGES — Country/CaseType pairs whose only change
        // is in CountryExp / CountryExpDelete (no CountryLaw remarks change).
        // Renders as a flat one-row-per-change table with changed cells
        // highlighted, instead of the two-row To/From layout used inside a
        // full country block. A single tax-term tweak isn't worth a page.
        // ═══════════════════════════════════════════════════════════════
        private void WriteOrphanExpirationChanges(Document doc, MdbComparisonResult comp,
            string clT, string? expT, string? expDelT)
        {
            if (expT == null) return; // patent-only table; trademark has no expT
            var fullBlockKeys = CountryLawBlockKeys(comp, clT, expT, expDelT);
            bool IsOrphan(RowDiff r) => !fullBlockKeys.Contains((G(r, "Country"), G(r, "CaseType")));

            var orphanAdd = new List<RowDiff>();
            var orphanMod = new List<RowDiff>();
            var orphanDel = new List<RowDiff>();
            if (H(comp, expT))
            {
                var ed = comp.TableDiffs[expT];
                orphanAdd.AddRange(ed.AddedRows.Where(IsOrphan));
                orphanMod.AddRange(ed.ModifiedRows.Where(IsOrphan));
            }
            if (expDelT != null && H(comp, expDelT))
                orphanDel.AddRange(comp.TableDiffs[expDelT].AddedRows.Where(IsOrphan));

            if (!orphanAdd.Any() && !orphanMod.Any() && !orphanDel.Any()) return;

            EnsureOrphanBreak(doc);
            var orphanHdr = _isR8Plus ? "Other Expiration Term Changes" : "Other Expiration and Tax Term Changes";
            var orphanSub = _isR8Plus
                ? "The following expiration term changes have no corresponding Law Highlights changes."
                : "The following expiration / tax term changes have no corresponding Law Highlights changes.";
            doc.Add(P(12).SetFont(_b).SetUnderline().SetMarginTop(14)
                .Add(T(orphanHdr, _b, 12)));
            doc.Add(P(9).SetMarginTop(4)
                .Add(T(orphanSub, _r, 9)));

            var tbl = new Table(UnitValue.CreatePercentArray(new float[] { 15, 8, 12, 12, 10, 12, 11, 11 }))
                .UseAllAvailableWidth().SetMarginTop(6).SetFontSize(8);
            foreach (var h in new[] { "Country", "Case", "Type", "Based On", "Terms (y-m)", "Effective For", "from", "to" })
                tbl.AddHeaderCell(new Cell().Add(P(8).SetFont(_b).Add(T(h, _b, 8)))
                    .SetBackgroundColor(HdrBg).SetBorder(new SolidBorder(0.5f)).SetPadding(2));

            // Same highlighting rules as the law-actions orphan table:
            // identifier columns never yellow, data columns yellow on add or
            // when r.ChangedColumns names them on a mod, deletes red strikethrough.
            void Row(RowDiff r, string mode)
            {
                bool isAdd = mode == "add", isMod = mode == "mod", isDel = mode == "del";
                bool Ch(params string[] cols) =>
                    isAdd || (isMod && cols.Any(c => r.ChangedColumns.Contains(c)));

                void C(string v, bool highlight)
                {
                    var t = T(v ?? "", _r, 8);
                    if (isDel) t.SetLineThrough().SetFontColor(Red);
                    var cell = new Cell().Add(P(8).Add(t))
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f)).SetPadding(2);
                    if (highlight && !isDel) cell.SetBackgroundColor(Yellow);
                    tbl.AddCell(cell);
                }

                C($"{CN(G(r, "Country"))} ({G(r, "Country")})", false);
                C(G(r, "CaseType"), false);
                C(G(r, "Type"), Ch("Type"));
                C(G(r, "BasedOn"), Ch("BasedOn"));
                C($"{G(r, "Yr")} {G(r, "Mo")}", Ch("Yr", "Mo"));
                C(G(r, "EffBasedOn"), Ch("EffBasedOn"));
                C(FD(r, "EffStartDate"), Ch("EffStartDate"));
                C(FD(r, "EffEndDate"), Ch("EffEndDate"));
            }

            foreach (var r in orphanAdd.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "Type")))
                Row(r, "add");
            foreach (var r in orphanMod.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "Type")))
                Row(r, "mod");
            foreach (var r in orphanDel.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "Type")))
                Row(r, "del");

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
        // INLINE REMARKS DIFF — word-level. Tokenizes both old and new into
        // words + whitespace runs, computes the LCS, and highlights only the
        // new tokens that aren't in the LCS. So a single changed date inside
        // a paragraph highlights just that date, not the whole surrounding line.
        // ═══════════════════════════════════════════════════════════════
        private Paragraph InlineDiff(string oldText, string newText)
        {
            var p = P(9);
            foreach (var seg in WordDiffSegments(oldText, newText))
            {
                var t = T(seg.text, _r, 9);
                // Yellow + underline so the change still reads on grayscale prints.
                if (seg.changed) t.SetBackgroundColor(Yellow).SetUnderline();
                p.Add(t);
            }
            return p;
        }

        // Word-level diff used by InlineDiff and AppendGreenLineDiff.
        // Returns the new text as a sequence of runs, each marked changed=true
        // if it was inserted/modified relative to oldText. Consecutive
        // same-state tokens are merged so one changed span = one styled run.
        private static List<(string text, bool changed)> WordDiffSegments(string? oldText, string? newText)
        {
            var oldTokens = Tokenize(oldText);
            var newTokens = Tokenize(newText);
            if (newTokens.Count == 0) return new List<(string, bool)>();

            var inLcs = LongestCommonSubsequence(oldTokens, newTokens);
            var changed = new bool[newTokens.Count];

            // Count non-whitespace tokens + matches to decide if the paragraph
            // is "mostly rewritten". Pure LCS on heavily-reworded legal prose
            // catches coincidental matches on stopwords ("the", "of", "a") and
            // produces swiss-cheese highlighting. If less than half the new
            // non-ws tokens survive in the LCS, just flag the whole thing.
            int totalNonWs = 0, matchedNonWs = 0;
            for (int i = 0; i < newTokens.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(newTokens[i])) continue;
                totalNonWs++;
                if (inLcs[i]) matchedNonWs++;
            }
            bool mostlyRewritten = oldTokens.Count == 0
                || (totalNonWs > 0 && matchedNonWs * 2 < totalNonWs);

            if (mostlyRewritten)
            {
                for (int i = 0; i < newTokens.Count; i++) changed[i] = true;
            }
            else
            {
                for (int i = 0; i < newTokens.Count; i++) changed[i] = !inLcs[i];

                // Coalesce: a short run of unchanged non-ws tokens (≤ 5) that
                // sits between two changed runs is almost always LCS noise on
                // common words — flip it to changed so the edit renders as
                // one contiguous yellow block instead of swiss cheese.
                const int MAX_GAP_NON_WS = 5;
                int k = 0;
                while (k < newTokens.Count)
                {
                    if (changed[k]) { k++; continue; }
                    int start = k, nonWs = 0;
                    while (k < newTokens.Count && !changed[k])
                    {
                        if (!string.IsNullOrWhiteSpace(newTokens[k])) nonWs++;
                        k++;
                    }
                    int end = k;
                    bool sandwiched = start > 0 && end < newTokens.Count;
                    if (sandwiched && nonWs > 0 && nonWs <= MAX_GAP_NON_WS)
                        for (int j = start; j < end; j++) changed[j] = true;
                }
            }

            // Don't highlight a whitespace-only token on its own. Only keep it
            // highlighted if both adjacent non-ws tokens are also highlighted —
            // that way an inserted phrase stays one contiguous yellow block,
            // but bare spaces between unchanged words don't flash.
            for (int i = 0; i < newTokens.Count; i++)
            {
                if (!changed[i] || !string.IsNullOrWhiteSpace(newTokens[i])) continue;
                bool leftHl = i > 0 && changed[i - 1] && !string.IsNullOrWhiteSpace(newTokens[i - 1]);
                bool rightHl = i < newTokens.Count - 1 && changed[i + 1] && !string.IsNullOrWhiteSpace(newTokens[i + 1]);
                if (!(leftHl && rightHl)) changed[i] = false;
            }

            var segments = new List<(string, bool)>();
            var sb = new System.Text.StringBuilder();
            bool? state = null;
            for (int i = 0; i < newTokens.Count; i++)
            {
                if (state != changed[i])
                {
                    if (sb.Length > 0) { segments.Add((sb.ToString(), state == true)); sb.Clear(); }
                    state = changed[i];
                }
                sb.Append(newTokens[i]);
            }
            if (sb.Length > 0) segments.Add((sb.ToString(), state == true));
            return segments;
        }

        // True if the release name targets only R8+ systems. We scan for "R<n>"
        // tokens and require every n found to be ≥ 8. Mixed-tier names like
        // "TmkR4-R8" or "Pat2000" return false so they keep the legacy
        // "Expiration and Tax Terms" wording.
        private static bool IsR8PlusName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var matches = System.Text.RegularExpressions.Regex.Matches(name, @"R(\d+)");
            if (matches.Count == 0) return false;
            foreach (System.Text.RegularExpressions.Match m in matches)
                if (!int.TryParse(m.Groups[1].Value, out var n) || n < 8) return false;
            return true;
        }

        // Split into tokens that are either a run of whitespace (incl. newlines)
        // or a run of non-whitespace. Keeping whitespace as its own token lets
        // us re-emit the text with original spacing intact.
        private static List<string> Tokenize(string? s) =>
            string.IsNullOrEmpty(s)
                ? new List<string>()
                : System.Text.RegularExpressions.Regex.Matches(s, @"\s+|\S+")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Value).ToList();

        // Classic LCS dynamic-programming table, then backtrack to mark which
        // tokens in the NEW sequence are part of the common subsequence.
        // Tokens not in the LCS are inserted/changed and should be highlighted.
        private static bool[] LongestCommonSubsequence(List<string> oldTokens, List<string> newTokens)
        {
            int n = oldTokens.Count, m = newTokens.Count;
            var inLcs = new bool[m];
            if (n == 0 || m == 0) return inLcs;
            var dp = new int[n + 1, m + 1];
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    dp[i, j] = oldTokens[i - 1] == newTokens[j - 1]
                        ? dp[i - 1, j - 1] + 1
                        : Math.Max(dp[i - 1, j], dp[i, j - 1]);
            int ii = n, jj = m;
            while (ii > 0 && jj > 0)
            {
                if (oldTokens[ii - 1] == newTokens[jj - 1]) { inLcs[jj - 1] = true; ii--; jj--; }
                else if (dp[ii - 1, jj] >= dp[ii, jj - 1]) ii--;
                else jj--;
            }
            return inLcs;
        }

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
