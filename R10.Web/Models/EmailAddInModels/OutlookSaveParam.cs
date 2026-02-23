namespace R10.Core.DTOs
{
    public class OutlookSaveParam
    {
        public int? cpiEmailId { get; set; }                 // this is the Id assigned by CPi, after logging; we need this since olItemId changes
        public string userEmail { get; set; }
        public string accessToken { get; set; }
        public string olItemId { get; set; }                // Outlook item id; note, Outlook id changes value when message is move between folders
        public string systemType { get; set; }
        public string[] inlineAttachments { get; set; }
        public string[] regularAttachments { get; set; }
        public object[] droppedAttachments { get; set; }
        public string screenCode { get; set; }
        public KeyTextDTO[] selectedCases { get; set; }

        public KeyTextDTO[] selectedCasesPaths { get; set; }
    }
}
