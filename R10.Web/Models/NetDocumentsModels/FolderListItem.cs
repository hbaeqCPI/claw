namespace R10.Web.Models.NetDocumentsModels
{
    public class FolderListItem
    {
        public FolderListItem()
        {
            Id = "";
            Name = "";
        }

        public FolderListItem(string? id, string? name, int level)
        {
            Id = id;
            Name = name;
            Level = level;
        }

        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
    }
}
