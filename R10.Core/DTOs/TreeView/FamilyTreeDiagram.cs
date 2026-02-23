using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    public class FamilyTreeDiagram
    {
        public List<BaseFamilyTreeDiagramDTO> Nodes { get; set; } = new List<BaseFamilyTreeDiagramDTO>();
        public List<FamilyTreeDiagramEdge> Edges { get; set; } = new List<FamilyTreeDiagramEdge>();
        public FamilyTreeDiagramHeader Header { get; set; } = new FamilyTreeDiagramHeader();
        public FamilyTreeDiagramStats Stats { get; set; } = new FamilyTreeDiagramStats();
        public FamilyTreeDiagramArea Area { get; set; }
    }

    public enum FamilyTreeDiagramArea
    {
        Regular, TerminalDisclaimer, TradeSecret
    }

    public class FamilyTreeDiagramHeader
    {
        public string? CaseNumber { get; set; }
        public string? LabelCaseNumber { get; set; }
        public List<string> Title { get; set; } = new List<string>();
        public string? LabelTitle { get; set; }
        public List<string> Client { get; set; } = new List<string>();
        public string? LabelClient { get; set; }
        public DateTime? ExpDate { get; set; }
        public string? LabelExpDate { get; set; }
    }

    public class FamilyTreeDiagramStats
    {
        public int ActiveCount { get; set; } = 0;
        public int InactiveCount { get; set; } = 0;

        // PatentUse
        public List<string> PriNumber { get; set; } = new List<string>();
        public List<string> PriCountry { get; set; } = new List<string>();
        public List<DateTime?> PriDate { get; set; } = new List<DateTime?>();
        public int ValidatedApps { get; set; } = 0;
        public int ExpiredPatents { get; set; } = 0;


        // TrademarkUse
        public string? MarkType { get; set; }
        public string? Classes { get; set; }
        public DateTime? FilDate { get; set; }
        public bool MadridProtocol { get; set; } = false;
        public bool EuropeanUnion { get; set; } = false;

    }
}
