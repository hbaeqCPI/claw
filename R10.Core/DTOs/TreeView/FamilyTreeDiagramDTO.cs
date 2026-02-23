using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(FamilyTreeDiagramDTO), "FamilyTree")]
    public class BaseFamilyTreeDiagramDTO
    {
        public string Id { get; set; } = "0";
        public int KeyId { get; set; } = 0;
        public string? Text { get; set; }
        public string? Title { get; set; } = "";
        public string? Type { get; set; } = "F"; //F Family or TrademarkName, I Invention, C CountryApplication, T Trademark
        public string? CaseNumber { get; set; }
        public bool IsStartingNode { get; set; } = false;

        // for TD
        [Display(Name = "Application No.")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public string? AppNumber { get; set; }

        public DateTime? Date { get; set; }

        [Display(Name = "Terminal Disclaimer")]
        [TargetedSystem(TargetedSystem.C)]
        public bool TerminalDisclaimer { get; set; } = false;

        [Display(Name = "Patent Term Adjustment")]
        [TargetedSystem(TargetedSystem.C)]
        public short? PatentTermAdj { get; set; } = 0;

        [Display(Name = "Filing Date")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Issue Date")]
        [TargetedSystem(TargetedSystem.C)]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Expiration Date")]
        [TargetedSystem(TargetedSystem.C)]
        public DateTime? ExpDate { get; set; }

        public BaseFamilyTreeDiagramDTO? ForwardPendingDate { get; set; }
        public BaseFamilyTreeDiagramDTO? BackwardPendingDate { get; set; }
        public bool Modified { get; set; } = false;
    }


    public class FamilyTreeDiagramDTO : BaseFamilyTreeDiagramDTO
    {
        public bool Active { get; set; } = false;

        public string? Status { get; set; }

        // Data display section
        [Display(Name = "Case Type")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public string? CaseType { get; set; }

        [Display(Name = "Client")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public string? Client { get; set; }

        [Display(Name = "Parent Application No.")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filling Date")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public DateTime? ParentFilDate { get; set; }

        [Display(Name = "Publication No.")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        [TargetedSystem(TargetedSystem.T, TargetedSystem.C)]
        public DateTime? PubDate { get; set; }

        // C only
        [Display(Name = "Patent No.")]
        [TargetedSystem(TargetedSystem.C)]
        public string? PatNumber { get; set; }

        [Display(Name = "Parent Patent No.")]
        [TargetedSystem(TargetedSystem.C)]
        public string? ParentPatNumber { get; set; }

        [Display(Name = "Parent Issue Date")]
        [TargetedSystem(TargetedSystem.C)]
        public DateTime? ParentIssDate { get; set; }

        [Display(Name = "PCT No.")]
        [TargetedSystem(TargetedSystem.C)]
        public string? PCTNumber { get; set; }

        [Display(Name = "PCT Date")]
        [TargetedSystem(TargetedSystem.C)]
        public DateTime? PCTDate { get; set; }

        [Display(Name = "Owners")]
        [TargetedSystem(TargetedSystem.C)]
        public string? Owners { get; set; }

        [Display(Name = "Inventors")]
        [TargetedSystem(TargetedSystem.C)]
        public string? Inventors { get; set; }

        // T only
        [Display(Name = "Registration  No.")]
        [TargetedSystem(TargetedSystem.T)]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration  Date")]
        [TargetedSystem(TargetedSystem.T)]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Last Renewal Date")]
        [TargetedSystem(TargetedSystem.T)]
        public DateTime? LastRenewalDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        [TargetedSystem(TargetedSystem.T)]
        public DateTime? NextRenewalDate { get; set; }

        [Display(Name = "Classes")]
        [TargetedSystem(TargetedSystem.T)]
        public string? Classes { get; set; }

        [Display(Name = "Country")]
        [TargetedSystem(TargetedSystem.T)]
        public string? Country { get; set; }

    }


    public enum TargetedSystem
    {
        T, C
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TargetedSystemAttribute : Attribute
    {
        public TargetedSystem[] Systems { get; }

        public TargetedSystemAttribute(params TargetedSystem[] systems)
        {
            Systems = systems;
        }
    }


    public class RelationshipNode
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
    }


    public class FamilyTreeDialogModel
    {
        public int Id { get; set; } = 0;
        public string ModalType { get; set; } = "Initialization";
        public string? Type { get; set; } = string.Empty;
        public string? CaseNumber { get; set; }
        public string? RecordTitle { get; set; } = string.Empty;
        public bool CanModify { get; set; } = false;
        [AllowNull]
        public string? Remarks { get; set; } = string.Empty;
        [AllowNull]
        public List<RelationshipNode>? DirectParents { get; set; } = [];
        [AllowNull]
        public List<RelationshipNode>? DirectChildren { get; set; } = [];

        [Display(Name = "Parent Reference")]
        public int? ParentId { get; set; }
        public string? ParentCase { get; set; }

    }

    public class FamilyTreeParentCaseDTO
    {
        public string? ParentCase { get; set; }
        public int ParentId { get; set; }
    }

}
