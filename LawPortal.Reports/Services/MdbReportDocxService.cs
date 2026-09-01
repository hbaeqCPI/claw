using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LawPortal.Reports.Services
{
    /// <summary>
    /// Word (.docx) renderer for the Country Law Update report. Mirrors
    /// <see cref="MdbReportPdfService"/> section-for-section, swapping iText
    /// primitives for OpenXML ones so the report can be downloaded as an
    /// editable Word document instead of a PDF.
    /// </summary>
    public class MdbReportDocxService
    {
        // Colors (hex, no leading '#').
        private const string Yellow = "FFFF00";
        private const string Red = "DC3232";
        private const string Green = "008200";
        private const string Blue = "0000FF";
        private const string White = "FFFFFF";
        private const string HdrBg = "EBEBEB";
        private const string LightGray = "D3D3D3";

        private const string FontName = "Arial"; // closest Word match to Helvetica

        // Letter page, margins matching the PDF (top 60, right/left 54, bottom 45 pt).
        private const int PageW = 12240, PageH = 15840;
        private const int MarginL = 1080, MarginR = 1080; // 54pt
        private const int ContentWidth = PageW - MarginL - MarginR; // 10080 twips

        private Dictionary<string, string> _cn = new(), _ctd = new();
        private bool _orphanBreakAdded;
        private bool _isR8Plus;
        private bool _leadingContent;
        private Body _body = null!;

        public byte[] GenerateReport(MdbComparisonResult comp, string name, string year, string qtr,
            Dictionary<string, string>? cn = null, Dictionary<string, string>? ctd = null,
            string? reportNotes = null, bool includeManualUpdates = true)
        {
            _cn = cn ?? new();
            _ctd = ctd ?? new();
            _orphanBreakAdded = false;
            _leadingContent = false;
            _isR8Plus = IsR8PlusName(name);

            using var ms = new MemoryStream();
            using (var word = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                var mainPart = word.AddMainDocumentPart();
                mainPart.Document = new Document();
                _body = new Body();
                mainPart.Document.With(_body);

                var title = comp.IsPatent ? "Patent Law Update" : "Trademark Law Update";
                var headerRefId = AddRunningHeader(mainPart, title);

                var pfx = comp.IsPatent ? "tblPat" : "tblTmk";
                var dueT = $"{pfx}CountryDue";
                var clT = $"{pfx}CountryLaw";
                var atT = $"{pfx}ActionType";
                var paramT = $"{pfx}ActionParameter";
                var expT = comp.IsPatent ? "tblPatCountryExp" : null;
                var expDelT = comp.IsPatent ? "tblPatCountryExpDelete" : null;

                WriteTitle(year, qtr);
                WriteReportNotes(reportNotes);
                WriteCountriesDeleted(comp, pfx);

                // The "Include Action Types" toggle suppresses ONLY the Manual
                // Updates (Action Type) section. Every other section — Structural,
                // Standard Goods, and the per-country Country Law blocks (Law
                // Actions / Law Highlights / Expiration) — is driven by unrelated
                // tables and always renders.
                if (comp.IsPatent)
                {
                    if (includeManualUpdates) WriteManualUpdates(comp, atT, dueT, paramT);
                    WriteStructural(comp, pfx);
                }
                else
                {
                    if (includeManualUpdates) WriteManualUpdates(comp, atT, dueT, paramT);
                    WriteStandardGoods(comp);
                    WriteStructural(comp, pfx);
                }

                WriteCountryLawAddedModified(comp, year, qtr, clT, dueT, expT, expDelT);
                WriteCountryLawDeleted(comp, clT);
                WriteOrphanLawActions(comp, clT, dueT, expT, expDelT);

                // Final section properties (page size / margins / running header).
                _body.With(SectionProperties(headerRefId));
                mainPart.Document.Save();
            }
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════
        // SECTION / HEADER
        // ═══════════════════════════════════════════════════════════════
        private static SectionProperties SectionProperties(string headerRefId)
        {
            var sect = new SectionProperties();
            sect.With(new HeaderReference { Type = HeaderFooterValues.Default, Id = headerRefId });
            sect.With(new PageSize { Width = (uint)PageW, Height = (uint)PageH });
            sect.With(new PageMargin
            {
                Top = 1200,
                Right = (uint)MarginR,
                Bottom = 900,
                Left = (uint)MarginL,
                Header = 720,
                Footer = 720,
                Gutter = 0
            });
            return sect;
        }

        private string AddRunningHeader(MainDocumentPart mainPart, string title)
        {
            var headerPart = mainPart.AddNewPart<HeaderPart>();
            var p = new Paragraph();
            var pp = new ParagraphProperties();
            pp.With(new ParagraphBorders(
                new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "000000", Space = 1 }));
            p.With(pp);
            p.With(Run(title, bold: true, italic: true, size: 11));
            headerPart.Header = new Header(p);
            headerPart.Header.Save();
            return mainPart.GetIdOfPart(headerPart);
        }

        // ═══════════════════════════════════════════════════════════════
        // REPORT NOTES
        // ═══════════════════════════════════════════════════════════════
        private void WriteReportNotes(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return;
            _leadingContent = true;

            var p = Para(spaceBeforePt: 10, boxBorder: true, shadeFill: "FCFCF0");
            var lines = notes.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                p.With(Run(lines[i], size: 10));
                if (i < lines.Length - 1) p.With(new Run(new Break()));
            }
            _body.With(p);
        }

        // ═══════════════════════════════════════════════════════════════
        // STANDARD GOODS (trademark only) — green underline for new/mod, red strike for del
        // ═══════════════════════════════════════════════════════════════
        private void WriteStandardGoods(MdbComparisonResult comp)
        {
            const string sgT = "tblTmkStandardGood";
            if (!H(comp, sgT)) return;
            var diff = comp.TableDiffs[sgT];
            if (!diff.AddedRows.Any() && !diff.ModifiedRows.Any() && !diff.DeletedRows.Any()) return;
            _leadingContent = true;

            _body.With(Para(spaceBeforePt: 10, jc: JustificationValues.Center)
                .With(Run("Nice Classification – Standard Goods Update", size: 16)));
            _body.With(Para(spaceBeforePt: 10)
                .With(Run("Records in the Standard Goods table under the Auxiliary Menu in your system are not added automatically. They will need to be adjusted by a person responsible for data entry.", size: 10)));

            _body.With(Para(spaceBeforePt: 8, indentPt: 20)
                .With(Run("1. Under the ", size: 10)).With(Run("Auxiliary", bold: true, size: 10))
                .With(Run(" menu, select ", size: 10)).With(Run("Standard Goods", bold: true, size: 10)));
            _body.With(Para(indentPt: 20).With(Run("2. Search and select each of the Classes below.", size: 10)));
            _body.With(Para(indentPt: 20)
                .With(Run("3. Click Edit, then paste the new description into the ", size: 10))
                .With(Run("Goods", bold: true, size: 10)).With(Run(" field.", size: 10)));
            _body.With(Para(indentPt: 20).With(Run("4. Click ", size: 10)).With(Run("Save.", bold: true, size: 10)));

            var tbl = NewTable(new float[] { 10, 90 }, 0, black: false, LightGray, spaceBeforePt: 10);
            tbl.Add(Cell(HdrBg, null, CellPara(Run("Class", bold: true, size: 9))));
            tbl.Add(Cell(HdrBg, null, CellPara(Run("Standard Goods", bold: true, size: 9))));

            var rows = diff.AddedRows.Select(r => (row: r, mode: "new"))
                .Concat(diff.ModifiedRows.Select(r => (row: r, mode: "mod")))
                .Concat(diff.DeletedRows.Select(r => (row: r, mode: "del")))
                .OrderBy(x => G(x.row, "Class")).ThenBy(x => G(x.row, "ClassType"))
                .ToList();

            foreach (var (row, mode) in rows)
            {
                var cls = G(row, "Class");
                var goods = G(row, "StandardGoods");

                tbl.Add(Cell(null, TableVerticalAlignmentValues.Top, CellPara(StyleChange(cls, mode))));

                var goodsPara = new Paragraph();
                if (mode == "new" || mode == "del")
                    goodsPara.With(StyleChange(goods, mode));
                else if (row.OldValues != null && row.ChangedColumns.Contains("StandardGoods"))
                {
                    var old = row.OldValues.ContainsKey("StandardGoods") ? row.OldValues["StandardGoods"]?.ToString() ?? "" : "";
                    AppendGreenLineDiff(goodsPara, old, goods);
                }
                else
                    goodsPara.With(Run(goods, size: 9));
                tbl.Add(Cell(null, TableVerticalAlignmentValues.Top, goodsPara));
            }
            AddTbl(tbl);
        }

        private Run StyleChange(string s, string mode) => mode switch
        {
            "new" or "mod" => Run(s, size: 9, color: Green, underline: true),
            "del" => Run(s, size: 9, color: Red, strike: true),
            _ => Run(s, size: 9)
        };

        private void AppendGreenLineDiff(Paragraph p, string oldText, string newText)
        {
            foreach (var seg in WordDiffSegments(oldText, newText))
                p.With(seg.changed ? Run(seg.text, size: 9, color: Green, underline: true) : Run(seg.text, size: 9));
        }

        // ═══════════════════════════════════════════════════════════════
        // TITLE
        // ═══════════════════════════════════════════════════════════════
        private void WriteTitle(string year, string qtr)
        {
            var tbl = NewTable(new float[] { 60, 40 }, 0, black: false, null, spaceBeforePt: 8);
            tbl.Add(Cell(null, null, CellPara(Run("Country Law Updates", bold: true, size: 22))));
            tbl.Add(Cell(null, null, new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                Run($"{year} – {qtr}", size: 18))));
            AddTbl(tbl);
        }

        // ═══════════════════════════════════════════════════════════════
        // MANUAL UPDATES
        // ═══════════════════════════════════════════════════════════════
        private void WriteManualUpdates(MdbComparisonResult comp, string atT, string dueT, string paramT)
        {
            if (!H(comp, atT)) return;
            var diff = comp.TableDiffs[atT];
            if (!diff.AddedRows.Any() && !diff.ModifiedRows.Any() && !diff.DeletedRows.Any()) return;
            _leadingContent = true;

            _body.With(Para(spaceBeforePt: 20, jc: JustificationValues.Center).With(Run("Manual Updates", size: 18)));
            _body.With(Para(spaceBeforePt: 14).With(Run("Records in the Action Type table under the Auxiliary or Maintenance Menu in your system are not automatically modified. They will need to be adjusted by a person responsible for data entry.", size: 10)));
            _body.With(Para(spaceBeforePt: 6).With(Run("Go to the Auxiliary or Maintenance Menu in your system and make the changes to the Action Types below if you do not already have them.", size: 10)));
            _body.With(Para(spaceBeforePt: 6)
                .With(Run("Please contact CPi at ", size: 10))
                .With(Run("countrylaw@computerpackages.com", size: 10, color: Blue, underline: true))
                .With(Run(" with any questions.", size: 10)));

            if (diff.AddedRows.Any())
            {
                _body.With(Para(spaceBeforePt: 14).With(Run("New Actions", bold: true, size: 11, underline: true)));
                foreach (var at in diff.AddedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(at, comp, dueT, paramT, "new");
            }
            if (diff.ModifiedRows.Any())
            {
                _body.With(Para(spaceBeforePt: 14).With(Run("Modified Actions", bold: true, size: 11, underline: true)));
                foreach (var at in diff.ModifiedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(at, comp, dueT, paramT, "mod");
            }
            if (diff.DeletedRows.Any())
            {
                _body.With(Para(spaceBeforePt: 14).With(Run("Deleted Actions:", bold: true, size: 11, underline: true)));
                foreach (var at in diff.DeletedRows.OrderBy(r => G(r, "ActionType")))
                    WriteActionTypeBlock(at, comp, dueT, paramT, "del");
            }
        }

        private void WriteActionTypeBlock(RowDiff at, MdbComparisonResult comp, string dueT, string paramT, string mode)
        {
            bool isNew = mode == "new", isDel = mode == "del";
            var name = G(at, "ActionType");
            var country = G(at, "Country");
            var isOA = G(at, "IsOfficeAction");
            var oaText = (isOA == "True" || isOA == "true" || isOA == "1" || isOA == "-1") ? "Yes" : "No";

            var ap = Para(spaceBeforePt: 10, indentPt: 30).With(Run("Action Type: ", size: 10));
            if (isDel) ap.With(Run($"{name} - DELETE", bold: true, size: 10, color: White, shadeFill: Red));
            else ap.With(Run(name, bold: true, size: 10, hl: isNew));
            ap.With(Run($"          Office Action: {oaText}", size: 10));
            _body.With(ap);

            if (!string.IsNullOrEmpty(country))
            {
                var cp = Para(spaceBeforePt: 2, indentPt: 50).With(Run("Country: ", size: 10));
                var label = $"{CN(country)} ({country})";
                cp.With(Run(label, size: 10, hl: isNew));
                _body.With(cp);
            }

            WriteActionParameterSubTable(at, comp, paramT, mode);

            var remarks = G(at, "Remarks");
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                var rt = NewTable(new float[] { 12, 88 }, 50, black: false, null, spaceBeforePt: 6);
                rt.Add(Cell(null, null, new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                    Run("Remarks:", size: 9))));
                Paragraph remCell;
                if (isNew)
                    remCell = CellPara(Run(remarks, size: 9, hl: true, underline: true));
                else if (isDel)
                    remCell = CellPara(Run(remarks, size: 9, strike: true));
                else if (at.OldValues != null && at.ChangedColumns.Contains("Remarks"))
                {
                    var old = at.OldValues.ContainsKey("Remarks") ? at.OldValues["Remarks"]?.ToString() ?? "" : "";
                    remCell = InlineDiff(old, remarks);
                }
                else
                    remCell = CellPara(Run(remarks, size: 9));
                rt.Add(Cell(null, null, remCell));
                AddTbl(rt);
            }

            var fuMsg = G(at, "FollowUpMsg");
            var fuMo = G(at, "FollowUpMonth");
            var fuDy = G(at, "FollowUpDay");
            var fuGen = G(at, "FollowUpGen");
            if (!string.IsNullOrWhiteSpace(fuMsg) || !string.IsNullOrEmpty(fuMo) || !string.IsNullOrEmpty(fuGen))
            {
                if (!string.IsNullOrWhiteSpace(fuMsg))
                    _body.With(Para(spaceBeforePt: 4, indentPt: 50).With(Run($"Follow Up Action: {fuMsg}", size: 9)));
                _body.With(Para(indentPt: 50).With(Run($"Follow Up Term: {fuMo} Month(s) / {fuDy} Day(s)", size: 9)));
                _body.With(Para(indentPt: 50).With(Run($"Follow Up Based On: {FollowUpGenLabel(fuGen)}", size: 9)));
            }

            HL();
        }

        private void WriteActionParameterSubTable(RowDiff at, MdbComparisonResult comp, string paramT, string parentMode)
        {
            if (!H(comp, paramT)) return;
            var atId = G(at, "ActionTypeID");
            if (string.IsNullOrEmpty(atId)) return;

            var pd = comp.TableDiffs[paramT];
            bool Matches(RowDiff r) => G(r, "ActionTypeID") == atId;

            var displayCols = new[] { "ActionDue", "Yr", "Mo", "Dy", "Indicator" };
            var added = pd.AddedRows.Where(Matches).ToList();
            var modified = pd.ModifiedRows.Where(Matches)
                .Where(r => displayCols.Any(c => r.ChangedColumns.Contains(c))).ToList();
            var deleted = pd.DeletedRows.Where(Matches).ToList();

            // A parameter's deadline term (ActionDue/Yr/Mo/Dy) is part of the
            // ActionParameter key, so editing it splits one edited row into a
            // deleted row + an added row. For a MODIFIED parent action type, pair
            // those back into a single modified row — matched on the ActionDue
            // label, which stays stable across a term edit — so the report shows
            // the change on one highlighted row instead of a struck-through
            // duplicate followed by a brand-new row.
            if (parentMode == "mod" && added.Any() && deleted.Any())
            {
                string Lbl(RowDiff r) => G(r, "ActionDue").Trim().ToLowerInvariant();
                foreach (var del in deleted.ToList())
                {
                    var match = added.FirstOrDefault(a => Lbl(a) == Lbl(del));
                    if (match == null) continue;
                    var changed = new HashSet<string>(
                        displayCols.Where(c => G(match, c).Trim() != G(del, c).Trim()));
                    modified.Add(new RowDiff
                    {
                        Values = match.Values,
                        OldValues = del.Values,
                        ChangedColumns = changed
                    });
                    added.Remove(match);
                    deleted.Remove(del);
                }
            }

            if (!added.Any() && !modified.Any() && !deleted.Any()) return;

            _body.With(Para(spaceBeforePt: 6, indentPt: 50).With(Run("Action Parameters", bold: true, size: 9)));

            var tbl = NewTable(new float[] { 40, 12, 12, 12, 24 }, 50, black: false, LightGray, spaceBeforePt: 3);
            foreach (var h in new[] { "Action Due", "Yr", "Mo", "Dy", "Indicator" })
                tbl.Add(Cell(HdrBg, null, CellPara(Run(h, bold: true, size: 8))));

            void Row(RowDiff r, string rowMode)
            {
                bool allYellow = rowMode == "add";
                bool allStrike = rowMode == "del";
                bool Ch(string col) => allYellow || (rowMode == "mod" && r.ChangedColumns.Contains(col));
                void C(string v, bool highlight, bool strike)
                {
                    var run = strike ? Run(v ?? "", size: 8, strike: true, color: Red) : Run(v ?? "", size: 8);
                    tbl.Add(Cell(highlight && !strike ? Yellow : null, null, CellPara(run)));
                }
                C(G(r, "ActionDue"), Ch("ActionDue"), allStrike);
                C(G(r, "Yr"), Ch("Yr"), allStrike);
                C(G(r, "Mo"), Ch("Mo"), allStrike);
                C(G(r, "Dy"), Ch("Dy"), allStrike);
                C(G(r, "Indicator"), Ch("Indicator"), allStrike);
            }

            string EffectiveMode(RowDiff r)
            {
                if (parentMode == "new") return "add";
                if (parentMode == "del") return "del";
                return r.OldValues == null ? "add" : "mod";
            }

            foreach (var r in added.OrderBy(r => G(r, "ActionDue"))) Row(r, EffectiveMode(r));
            foreach (var r in modified.OrderBy(r => G(r, "ActionDue"))) Row(r, EffectiveMode(r));
            foreach (var r in deleted.OrderBy(r => G(r, "ActionDue"))) Row(r, parentMode == "new" ? "add" : "del");

            AddTbl(tbl);
        }

        // Action Due table (shared by Manual Updates + Country Law).
        private void WriteActionDueTable(List<RowDiff> rows, bool treatAllAsNew = false,
            bool treatAllAsDeleted = false, float marginLeft = 20, bool includeCountry = false)
        {
            float[] cols = includeCountry
                ? new float[] { 14, 18, 5, 5, 5, 10, 10, 11, 11, 11 }
                : new float[] { 20, 5, 5, 5, 11, 11, 13, 13, 17 };
            var tbl = NewTable(cols, marginLeft, black: false, LightGray, spaceBeforePt: 6);
            if (includeCountry) tbl.Add(HCell("Country"));
            foreach (var h in new[] { "Action Due", "Yr", "Mo", "Dy", "Indicator", "Based On", "Effective From", "To", "Effective Based On" })
                tbl.Add(HCell(h));

            foreach (var row in rows
                .OrderBy(r => includeCountry ? CN(G(r, "Country")) : "")
                .ThenBy(r => G(r, "ActionDue")))
            {
                bool isAdd = row.OldValues == null;
                string countryLabel = includeCountry ? $"{CN(G(row, "Country"))} ({G(row, "Country")})" : "";
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
                    MC(tbl, row, "Dy"); MC(tbl, row, "Indicator"); MC(tbl, row, "BasedOn");
                    MCDate(tbl, row, "EffStartDate"); MCDate(tbl, row, "EffEndDate");
                    MC(tbl, row, "EffBasedOn");
                }
            }
            AddTbl(tbl);
        }

        private void MCDate(Tbl t, RowDiff row, string col)
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
        // COUNTRIES DELETED
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountriesDeleted(MdbComparisonResult comp, string pfx)
        {
            var cTbl = $"{pfx}Country";
            if (!H(comp, cTbl) || !comp.TableDiffs[cTbl].DeletedRows.Any()) return;

            SectionHeader("Countries Deleted");
            foreach (var r in comp.TableDiffs[cTbl].DeletedRows.OrderBy(r => CN(G(r, "Country"))))
                _body.With(Para(indentPt: 30)
                    .With(Run($"{CN(G(r, "Country"))} ({G(r, "Country")})", size: 9, strike: true, color: Red)));
        }

        // ═══════════════════════════════════════════════════════════════
        // STRUCTURAL: Areas, Case Types, Designations
        // ═══════════════════════════════════════════════════════════════
        private void WriteStructural(MdbComparisonResult c, string pfx)
        {
            var acTbl = $"{pfx}AreaCountry";
            var acDelTbl = $"{pfx}AreaCountryDelete";
            var ctTbl = $"{pfx}CaseType";
            var desTbl = $"{pfx}DesCaseType";
            var desDelTbl = $"{pfx}DesCaseTypeDelete";

            var acDel = new List<RowDiff>();
            if (H(c, acTbl)) acDel.AddRange(c.TableDiffs[acTbl].DeletedRows);
            if (H(c, acDelTbl)) acDel.AddRange(c.TableDiffs[acDelTbl].AddedRows);
            acDel = acDel.GroupBy(r => (G(r, "Area"), G(r, "Country"))).Select(g => g.First()).ToList();
            if (acDel.Any())
            {
                SectionHeader("Area Countries Deleted");
                foreach (var g in acDel.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    _body.With(Para(spaceBeforePt: 4).With(Run(g.Key, bold: true, size: 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        _body.With(Para(indentPt: 30)
                            .With(Run($"{CN(G(r, "Country"))} ({G(r, "Country")})", size: 9, strike: true, color: Red)));
                }
            }

            if (H(c, acTbl) && c.TableDiffs[acTbl].AddedRows.Any())
            {
                SectionHeader("Area Countries Added");
                foreach (var g in c.TableDiffs[acTbl].AddedRows.GroupBy(r => G(r, "Area")).OrderBy(g => g.Key))
                {
                    _body.With(Para(spaceBeforePt: 4).With(Run(g.Key, bold: true, size: 10)));
                    foreach (var r in g.OrderBy(r => CN(G(r, "Country"))))
                        _body.With(Para(indentPt: 30)
                            .With(Run($"{CN(G(r, "Country"))} ({G(r, "Country")})", size: 9, hl: true)));
                }
            }

            if (H(c, ctTbl) && c.TableDiffs[ctTbl].AddedRows.Any())
            {
                if (c.IsPatent) PageBreak();
                SectionHeader("Case Types Added");
                foreach (var r in c.TableDiffs[ctTbl].AddedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = NewTable(new float[] { 15, 85 }, 0, black: false, null);
                    t.Add(Cell(null, null, new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        Run(G(r, "CaseType"), size: 9, hl: true))));
                    t.Add(Cell(null, null, CellPara(Run(G(r, "Description"), size: 9, hl: true))));
                    AddTbl(t);
                }
            }

            if (H(c, ctTbl) && c.TableDiffs[ctTbl].DeletedRows.Any())
            {
                SectionHeader("Case Types Deleted");
                foreach (var r in c.TableDiffs[ctTbl].DeletedRows.OrderBy(r => G(r, "CaseType")))
                {
                    var t = NewTable(new float[] { 15, 85 }, 0, black: false, null);
                    t.Add(Cell(null, null, new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        Run(G(r, "CaseType"), size: 9, strike: true, color: Red))));
                    t.Add(Cell(null, null, CellPara(Run(G(r, "Description"), size: 9, strike: true, color: Red))));
                    AddTbl(t);
                }
            }

            var desDel = new List<RowDiff>();
            if (H(c, desTbl)) desDel.AddRange(c.TableDiffs[desTbl].DeletedRows);
            if (H(c, desDelTbl)) desDel.AddRange(c.TableDiffs[desDelTbl].AddedRows);
            desDel = desDel
                .GroupBy(r => (G(r, "IntlCode"), G(r, "CaseType"), G(r, "DesCountry"), G(r, "DesCaseType")))
                .Select(g => g.First()).ToList();
            if (desDel.Any())
            {
                SectionHeader("Designation Deleted");
                WriteDesignations(desDel, added: false);
            }

            if (H(c, desTbl) && c.TableDiffs[desTbl].AddedRows.Any())
            {
                SectionHeader("Designation Added");
                WriteDesignations(c.TableDiffs[desTbl].AddedRows, added: true);
            }
        }

        private void WriteDesignations(List<RowDiff> rows, bool added)
        {
            Run Style(string s, bool bold) => added
                ? Run(s, bold: bold, size: 9, hl: true)
                : Run(s, bold: bold, size: 9, strike: true, color: Red);

            var tbl = NewTable(new float[] { 50, 50 }, 0, black: false, null, spaceBeforePt: 4);
            tbl.Add(Cell(null, null, CellPara(Run("Organization/Union", bold: true, size: 9))));
            tbl.Add(Cell(null, null, CellPara(Run("Type of filing (Case Type)", bold: true, size: 9))));

            foreach (var ioGrp in rows.GroupBy(r => G(r, "IntlCode")).OrderBy(g => CN(g.Key)))
            {
                foreach (var ctGrp in ioGrp.GroupBy(r => G(r, "CaseType")).OrderBy(g => g.Key))
                {
                    var ct = ctGrp.Key;
                    tbl.Add(Cell(null, null, Para(spaceBeforePt: 4).With(Style($"{CN(ioGrp.Key)} ({ioGrp.Key})", true))));
                    tbl.Add(Cell(null, null, Para(spaceBeforePt: 4).With(Style($"{ct} - {CD(ct)}", true))));
                    tbl.Add(Cell(null, null, CellPara(Run(added ? "Can be designated in" : "designated in", size: 9))));
                    tbl.Add(Cell(null, null, CellPara(Run("as", size: 9))));
                    foreach (var r in ctGrp.OrderBy(r => CN(G(r, "DesCountry"))))
                    {
                        tbl.Add(Cell(null, null, CellPara(Style($"{CN(G(r, "DesCountry"))} ({G(r, "DesCountry")})", false))));
                        tbl.Add(Cell(null, null, CellPara(Style(G(r, "DesCaseType"), false))));
                    }
                }
            }
            AddTbl(tbl);
        }

        private void SectionHeader(string text)
        {
            _leadingContent = true;
            _body.With(Para(spaceBeforePt: 14).With(Run(text, bold: true, size: 11, underline: true)));
        }

        // ═══════════════════════════════════════════════════════════════
        // COUNTRY LAW ADDED/MODIFIED
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountryLawAddedModified(MdbComparisonResult comp, string year, string qtr,
            string clT, string dueT, string? expT, string? expDelT)
        {
            var fullBlockKeys = CountryLawBlockKeys(comp, clT, expT, expDelT);
            if (!fullBlockKeys.Any()) return;

            if (_leadingContent)
            {
                PageBreak();
                WriteTitle(year, qtr);
            }
            _body.With(Para(spaceBeforePt: 6).With(Run("Country Law Added/Modified", bold: true, size: 11, underline: true)));

            var newClKeys = new HashSet<(string, string)>();
            if (H(comp, clT))
                foreach (var r in comp.TableDiffs[clT].AddedRows)
                    newClKeys.Add((G(r, "Country"), G(r, "CaseType")));

            bool first = true;
            foreach (var (country, caseType) in fullBlockKeys.OrderBy(k => CN(k.Item1)).ThenBy(k => k.Item2))
            {
                if (!first) PageBreak();
                first = false;
                bool isNewBlock = newClKeys.Contains((country, caseType));
                WriteCountryBlock(comp, country, caseType, isNewBlock, clT, dueT, expT, expDelT);
            }
        }

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

        private void WriteCountryBlock(MdbComparisonResult comp, string country, string caseType,
            bool isNewBlock, string clT, string dueT, string? expT, string? expDelT)
        {
            var hdr = NewTable(new float[] { 55, 45 }, 0, black: false, null, spaceBeforePt: 12);
            hdr.Add(Cell(null, null, CellPara(Run($"Country: {CN(country)} ({country})", bold: true, italic: true, size: 11, hl: isNewBlock))));
            hdr.Add(Cell(null, null, new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                Run($"{caseType} - {CD(caseType)}", bold: true, italic: true, size: 11, hl: isNewBlock))));
            AddTbl(hdr);

            if (expT != null && H(comp, expT))
            {
                var ed = comp.TableDiffs[expT];
                var added = ed.AddedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                var modified = ed.ModifiedRows.Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                var hdrPrefix = _isR8Plus ? "Expiration Terms" : "Expiration and Tax Terms";
                if (added.Any())
                {
                    _body.With(Para(spaceBeforePt: 6).With(Run($"{hdrPrefix} Added", bold: true, size: 10, underline: true)));
                    WriteExpTable(added, mode: "add");
                }
                if (modified.Any())
                {
                    _body.With(Para(spaceBeforePt: 6).With(Run($"{hdrPrefix} Modified", bold: true, size: 10, underline: true)));
                    WriteExpTable(modified, mode: "mod");
                }
            }
            if (expDelT != null && H(comp, expDelT))
            {
                var del = comp.TableDiffs[expDelT].AddedRows
                    .Where(r => G(r, "Country") == country && G(r, "CaseType") == caseType).ToList();
                if (del.Any())
                {
                    var hdrPrefix = _isR8Plus ? "Expiration Terms" : "Expiration and Tax Terms";
                    _body.With(Para(spaceBeforePt: 6).With(Run($"{hdrPrefix} Deleted", bold: true, size: 10, underline: true)));
                    WriteExpTable(del, mode: "del");
                }
            }

            if (H(comp, dueT))
            {
                var dd = comp.TableDiffs[dueT];
                bool Matches(RowDiff r) => G(r, "Country") == country && G(r, "CaseType") == caseType;
                var addMod = dd.AddedRows.Where(Matches).Concat(dd.ModifiedRows.Where(Matches)).ToList();
                var deleted = dd.DeletedRows.Where(Matches).ToList();

                if (addMod.Any() || deleted.Any())
                    _body.With(Para(spaceBeforePt: 6).With(Run("Law Actions", bold: true, size: 10, underline: true)));
                if (addMod.Any())
                    WriteActionDueTable(addMod, treatAllAsNew: isNewBlock, marginLeft: 10);
                if (deleted.Any())
                {
                    _body.With(Para(spaceBeforePt: 4, indentPt: 10).With(Run("Deleted:", italic: true, size: 9)));
                    WriteActionDueTable(deleted, treatAllAsDeleted: true, marginLeft: 10);
                }
            }

            if (H(comp, clT))
            {
                var cd = comp.TableDiffs[clT];
                var clRow = cd.AddedRows.Concat(cd.ModifiedRows)
                    .FirstOrDefault(r => G(r, "Country") == country && G(r, "CaseType") == caseType);
                if (clRow != null && !string.IsNullOrWhiteSpace(G(clRow, "Remarks")))
                {
                    _body.With(Para(spaceBeforePt: 8).With(Run("Law Highlights", bold: true, size: 10, underline: true)));
                    var remarks = G(clRow, "Remarks");
                    if (isNewBlock || clRow.OldValues == null || !clRow.ChangedColumns.Contains("Remarks"))
                    {
                        var rP = Para(spaceBeforePt: 3, indentPt: 20);
                        rP.With(isNewBlock ? Run(remarks, size: 9, hl: true, underline: true) : Run(remarks, size: 9));
                        _body.With(rP);
                    }
                    else
                    {
                        var old = clRow.OldValues.ContainsKey("Remarks") ? clRow.OldValues["Remarks"]?.ToString() ?? "" : "";
                        var p = InlineDiff(old, remarks);
                        p.ParagraphProperties ??= new ParagraphProperties();
                        p.ParagraphProperties.With(new SpacingBetweenLines { Before = "60" });
                        p.ParagraphProperties.With(new Indentation { Left = "400" });
                        _body.With(p);
                    }
                }
            }
        }

        // Expiration & Tax Terms table (headers aligned to the data columns:
        // Type, Based On, Terms (y-m), Effective For, from, to).
        private void WriteExpTable(List<RowDiff> rows, string mode)
        {
            bool isMod = mode == "mod", isDel = mode == "del";
            var tbl = NewTable(new float[] { 12, 20, 16, 20, 16, 16 }, 10, black: false, null, spaceBeforePt: 3);

            foreach (var h in new[] { "Type", "Based On", "Terms (y-m-d)", "Effective For", "from", "to" })
                tbl.Add(Cell(null, null, CellPara(Run(h, bold: true, size: 9))));

            foreach (var row in rows.OrderBy(r => G(r, "Type")))
            {
                var typ = G(row, "Type");
                var bon = G(row, "BasedOn");
                // Include Dy in the term. It's a non-key body column, so a
                // day-only change registers as "Modified"; leaving it off the
                // display (previously "y-m") rendered that row with no highlighted
                // cell, making a real change look like an unchanged row.
                var terms = $"{G(row, "Yr")} {G(row, "Mo")} {G(row, "Dy")}";
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
                    bool Ch(params string[] c) => c.Any(x => row.ChangedColumns.Contains(x));
                    void Cell2(string v, bool y) { if (y) NBCellY(tbl, v); else NBCell(tbl, v); }
                    Cell2(typ, Ch("Type"));
                    Cell2(bon, Ch("BasedOn"));
                    Cell2(terms, Ch("Yr", "Mo", "Dy"));
                    Cell2(eff, Ch("EffBasedOn"));
                    Cell2(from, Ch("EffStartDate"));
                    Cell2(to, Ch("EffEndDate"));
                }
                else
                {
                    NBCellY(tbl, typ); NBCellY(tbl, bon); NBCellY(tbl, terms);
                    NBCellY(tbl, eff); NBCellY(tbl, from); NBCellY(tbl, to);
                }
            }
            AddTbl(tbl);
        }

        // ═══════════════════════════════════════════════════════════════
        // ORPHAN LAW ACTIONS
        // ═══════════════════════════════════════════════════════════════
        private void WriteOrphanLawActions(MdbComparisonResult comp, string clT, string dueT, string? expT, string? expDelT)
        {
            if (!H(comp, dueT)) return;
            var dd = comp.TableDiffs[dueT];
            var fullBlockKeys = CountryLawBlockKeys(comp, clT, expT, expDelT);

            bool IsOrphan(RowDiff r) => !fullBlockKeys.Contains((G(r, "Country"), G(r, "CaseType")));
            var orphanAdd = dd.AddedRows.Where(IsOrphan).ToList();
            // Only surface a modified row when a column this compact table actually
            // shows changed. CountryDue also carries columns this table doesn't
            // display (EffBasedOn, Recurring, CPIAction, Calculate, MultipleBasedOn);
            // a change confined to those would otherwise render a row with nothing
            // highlighted — indistinguishable from an unchanged row.
            var orphanDisplayCols = new[] { "ActionDue", "Indicator", "BasedOn", "Yr", "Mo", "Dy" };
            var orphanMod = dd.ModifiedRows.Where(IsOrphan)
                .Where(r => orphanDisplayCols.Any(c => r.ChangedColumns.Contains(c))).ToList();
            var orphanDel = dd.DeletedRows.Where(IsOrphan).ToList();
            if (!orphanAdd.Any() && !orphanMod.Any() && !orphanDel.Any()) return;

            EnsureOrphanBreak();
            _body.With(Para(spaceBeforePt: 6).With(Run("Other Law Action Changes", bold: true, size: 12, underline: true)));
            _body.With(Para(spaceBeforePt: 4).With(Run("The following law action changes have no corresponding Law Highlights changes.", size: 9)));

            var tbl = NewTable(new float[] { 17, 9, 27, 14, 8, 8, 8 }, 0, black: false, LightGray, spaceBeforePt: 6);
            foreach (var h in new[] { "Country", "Case", "Action Due (Indicator)", "Based On", "Yr", "Mo", "Dy" })
                tbl.Add(Cell(HdrBg, null, CellPara(Run(h, bold: true, size: 8))));

            void Row(RowDiff r, string mode)
            {
                bool isMod = mode == "mod", isDel = mode == "del", isAdd = mode == "add";
                bool Ch(params string[] cols) => isAdd || (isMod && cols.Any(c => r.ChangedColumns.Contains(c)));
                void C(string v, bool highlight)
                {
                    var run = isDel ? Run(v ?? "", size: 8, strike: true, color: Red) : Run(v ?? "", size: 8);
                    tbl.Add(Cell(highlight && !isDel ? Yellow : null, null, CellPara(run)));
                }
                C($"{CN(G(r, "Country"))} ({G(r, "Country")})", false);
                C(G(r, "CaseType"), false);
                C($"{G(r, "ActionDue")} ({G(r, "Indicator")})", Ch("ActionDue", "Indicator"));
                C(G(r, "BasedOn"), Ch("BasedOn"));
                C(G(r, "Yr"), Ch("Yr"));
                C(G(r, "Mo"), Ch("Mo"));
                C(G(r, "Dy"), Ch("Dy"));
            }

            foreach (var r in orphanAdd.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue"))) Row(r, "add");
            foreach (var r in orphanMod.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue"))) Row(r, "mod");
            foreach (var r in orphanDel.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")).ThenBy(r => G(r, "ActionDue"))) Row(r, "del");

            AddTbl(tbl);
        }

        private void EnsureOrphanBreak()
        {
            if (_orphanBreakAdded) return;
            PageBreak();
            _orphanBreakAdded = true;
        }

        // ═══════════════════════════════════════════════════════════════
        // COUNTRY LAW DELETED
        // ═══════════════════════════════════════════════════════════════
        private void WriteCountryLawDeleted(MdbComparisonResult comp, string clT)
        {
            if (!H(comp, clT) || !comp.TableDiffs[clT].DeletedRows.Any()) return;
            _body.With(Para(spaceBeforePt: 16).With(Run("Country Law Deleted", bold: true, size: 12, underline: true)));
            foreach (var r in comp.TableDiffs[clT].DeletedRows.OrderBy(r => CN(G(r, "Country"))).ThenBy(r => G(r, "CaseType")))
                _body.With(Para(indentPt: 10)
                    .With(Run($"Country: {CN(G(r, "Country"))} ({G(r, "Country")})     {G(r, "CaseType")} - {CD(G(r, "CaseType"))}", size: 10)));
        }

        // ═══════════════════════════════════════════════════════════════
        // INLINE REMARKS DIFF (word-level, same algorithm as the PDF service)
        // ═══════════════════════════════════════════════════════════════
        private Paragraph InlineDiff(string oldText, string newText)
        {
            var p = new Paragraph();
            foreach (var seg in WordDiffSegments(oldText, newText))
                p.With(seg.changed ? Run(seg.text, size: 9, hl: true, underline: true) : Run(seg.text, size: 9));
            return p;
        }

        private static List<(string text, bool changed)> WordDiffSegments(string? oldText, string? newText)
        {
            var oldTokens = Tokenize(oldText);
            var newTokens = Tokenize(newText);
            if (newTokens.Count == 0) return new List<(string, bool)>();

            var inLcs = LongestCommonSubsequence(oldTokens, newTokens);
            var changed = new bool[newTokens.Count];

            int totalNonWs = 0, matchedNonWs = 0;
            for (int i = 0; i < newTokens.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(newTokens[i])) continue;
                totalNonWs++;
                if (inLcs[i]) matchedNonWs++;
            }
            bool mostlyRewritten = oldTokens.Count == 0 || (totalNonWs > 0 && matchedNonWs * 2 < totalNonWs);

            if (mostlyRewritten)
            {
                for (int i = 0; i < newTokens.Count; i++) changed[i] = true;
            }
            else
            {
                for (int i = 0; i < newTokens.Count; i++) changed[i] = !inLcs[i];

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

        private static bool IsR8PlusName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var matches = System.Text.RegularExpressions.Regex.Matches(name, @"R(\d+)");
            if (matches.Count == 0) return false;
            foreach (System.Text.RegularExpressions.Match m in matches)
                if (!int.TryParse(m.Groups[1].Value, out var n) || n < 8) return false;
            return true;
        }

        private static List<string> Tokenize(string? s) =>
            string.IsNullOrEmpty(s)
                ? new List<string>()
                : System.Text.RegularExpressions.Regex.Matches(s, @"\s+|\S+")
                    .Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).ToList();

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
        // DATA HELPERS
        // ═══════════════════════════════════════════════════════════════
        private static string G(RowDiff r, string c) => r.Values.ContainsKey(c) ? r.Values[c]?.ToString() ?? "" : "";
        private string CN(string c) => _cn.ContainsKey(c) ? _cn[c] : c;
        private string CD(string c) => _ctd.ContainsKey(c) ? _ctd[c] : c;
        private static string FD(RowDiff r, string c)
        {
            var v = G(r, c);
            return DateTime.TryParse(v, out var d) ? d.ToString("M/d/yyyy") : v;
        }
        private static bool H(MdbComparisonResult c, string t) => c.TableDiffs.ContainsKey(t);

        // ═══════════════════════════════════════════════════════════════
        // OPENXML PRIMITIVES
        // ═══════════════════════════════════════════════════════════════
        private static Run Run(string s, bool bold = false, bool italic = false, float size = 9,
            string? color = null, bool underline = false, bool strike = false, bool hl = false, string? shadeFill = null)
        {
            var rp = new RunProperties();
            rp.With(new RunFonts { Ascii = FontName, HighAnsi = FontName });
            if (bold) rp.With(new Bold());
            if (italic) rp.With(new Italic());
            if (strike) rp.With(new Strike());
            if (color != null) rp.With(new Color { Val = color });
            rp.With(new FontSize { Val = ((int)(size * 2)).ToString() });
            if (hl) rp.With(new Highlight { Val = HighlightColorValues.Yellow });
            if (underline) rp.With(new Underline { Val = UnderlineValues.Single });
            if (shadeFill != null) rp.With(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = shadeFill });

            var run = new Run(rp);
            // Preserve the original formatting of multi-line / tabbed text (e.g.
            // Remarks and Law Highlights): a bare <w:t> collapses newlines and tabs,
            // so emit <w:br/> per line break and <w:tab/> per tab. Single-line values
            // produce exactly one <w:t>, unchanged from before.
            var text = (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
            bool firstLine = true;
            foreach (var line in text.Split('\n'))
            {
                if (!firstLine) run.With(new Break());
                firstLine = false;
                bool firstTab = true;
                foreach (var seg in line.Split('\t'))
                {
                    if (!firstTab) run.With(new TabChar());
                    firstTab = false;
                    run.With(new Text(seg) { Space = SpaceProcessingModeValues.Preserve });
                }
            }
            return run;
        }

        private static Paragraph Para(float spaceBeforePt = 0, float indentPt = 0, JustificationValues? jc = null,
            bool boxBorder = false, string? shadeFill = null)
        {
            var p = new Paragraph();
            var pp = new ParagraphProperties();
            if (boxBorder)
                pp.With(new ParagraphBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4, Color = "808080", Space = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "808080", Space = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "808080", Space = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4, Color = "808080", Space = 4 }));
            if (shadeFill != null) pp.With(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = shadeFill });
            if (spaceBeforePt > 0) pp.With(new SpacingBetweenLines { Before = ((int)(spaceBeforePt * 20)).ToString() });
            if (indentPt > 0) pp.With(new Indentation { Left = ((int)(indentPt * 20)).ToString() });
            if (jc != null) pp.With(new Justification { Val = jc.Value });
            if (pp.HasChildren) p.With(pp);
            return p;
        }

        private static Paragraph CellPara(params Run[] runs)
        {
            var p = new Paragraph();
            foreach (var r in runs) p.With(r);
            return p;
        }

        private void PageBreak() => _body.With(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

        private void HL() =>
            _body.With(new Paragraph(new ParagraphProperties(
                new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 4, Color = LightGray, Space = 1 }),
                new SpacingBetweenLines { Before = "80", After = "80" })));

        // ── Tables ──────────────────────────────────────────────────────
        private sealed class Tbl
        {
            public readonly Table Table;
            private readonly int[] _w;
            private int _i;
            private TableRow? _row;
            public Tbl(Table t, int[] w) { Table = t; _w = w; }
            public void Add(TableCell cell)
            {
                if (_row == null || _i >= _w.Length) { _row = new TableRow(); Table.With(_row); _i = 0; }
                var pr = cell.GetFirstChild<TableCellProperties>();
                if (pr == null) { pr = new TableCellProperties(); cell.InsertAt(pr, 0); }
                pr.InsertAt(new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = _w[_i].ToString() }, 0);
                _row.With(cell);
                _i++;
            }
        }

        private static Tbl NewTable(float[] pct, float marginLeftPt, bool black, string? borderColor, float spaceBeforePt = 0)
        {
            int indent = (int)(marginLeftPt * 20);
            int usable = ContentWidth - indent;
            var widths = new int[pct.Length];
            for (int i = 0; i < pct.Length; i++) widths[i] = (int)(pct[i] / 100f * usable);

            var t = new Table();
            var tp = new TableProperties();
            tp.With(new TableWidth { Type = TableWidthUnitValues.Dxa, Width = widths.Sum().ToString() });
            if (indent > 0) tp.With(new TableIndentation { Type = TableWidthUnitValues.Dxa, Width = indent });
            if (borderColor != null) tp.With(Borders(borderColor, 4));
            tp.With(new TableLayout { Type = TableLayoutValues.Fixed });
            t.With(tp);

            var grid = new TableGrid();
            foreach (var w in widths) grid.With(new GridColumn { Width = ((uint)w).ToString() });
            t.With(grid);

            var tbl = new Tbl(t, widths);
            // spacing handled by an empty paragraph before the table if requested
            if (spaceBeforePt > 0) { /* spacing added by caller context via AddTbl's leading paragraph */ }
            return tbl;
        }

        private static TableBorders Borders(string color, uint size) => new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 },
            new LeftBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 },
            new BottomBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 },
            new RightBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = size, Color = color, Space = 0 });

        // Append a table to the body, wrapped by empty paragraphs so consecutive
        // tables don't merge (Word merges adjacent tables with no paragraph between).
        private void AddTbl(Tbl tbl)
        {
            _body.With(tbl.Table);
            _body.With(new Paragraph());
        }

        private static TableCell Cell(string? shadeFill, TableVerticalAlignmentValues? valign, params OpenXmlElement[] content)
        {
            var c = new TableCell();
            var pr = new TableCellProperties();
            if (shadeFill != null) pr.With(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = shadeFill });
            if (valign != null) pr.With(new TableCellVerticalAlignment { Val = valign.Value });
            c.With(pr);
            if (content.Length == 0) c.With(new Paragraph());
            else foreach (var el in content) c.With(el);
            return c;
        }

        // Header / data cell helpers for the bordered tables (mirror the PDF service).
        private static TableCell HCell(string t) => Cell(HdrBg, null, CellPara(Run(t, bold: true, size: 8)));
        private static void PC(Tbl t, string v) => t.Add(Cell(null, null, CellPara(Run(v ?? "", size: 8))));
        private static void YC(Tbl t, string v) => t.Add(Cell(Yellow, null, CellPara(Run(v ?? "", size: 8))));
        private static void DC(Tbl t, string v) => t.Add(Cell(null, null, CellPara(Run(v ?? "", size: 8, strike: true, color: Red))));
        private static void MC(Tbl t, RowDiff row, string col)
        {
            var v = G(row, col);
            if (row.ChangedColumns.Contains(col)) YC(t, v); else PC(t, v);
        }

        // No-border cell helpers (Expiration table).
        private static void NBCell(Tbl t, string v) => t.Add(Cell(null, null, CellPara(Run(v ?? "", size: 9))));
        private static void NBCellY(Tbl t, string v) => t.Add(Cell(Yellow, null, CellPara(Run(v ?? "", size: 9))));
        private static void NBCellDel(Tbl t, string v) => t.Add(Cell(null, null, CellPara(Run(v ?? "", size: 9, strike: true, color: Red))));
    }

    // OpenXML's Append(...) returns void; this fluent variant appends children
    // and returns the element so paragraph/run building can be chained.
    internal static class OoxmlFluent
    {
        public static T With<T>(this T element, params OpenXmlElement[] children) where T : OpenXmlElement
        {
            foreach (var c in children) element.Append(c);
            return element;
        }
    }
}
