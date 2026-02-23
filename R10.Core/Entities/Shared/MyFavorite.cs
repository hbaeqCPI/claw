using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities
{
    public class MyFavorite : BaseEntity
    {
        [Key]
        public int FavoriteId { get; set;}

        public string? SystemType { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public string? Author { get; set; }
    }

}
