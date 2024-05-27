using Microsoft.Build.Framework;
using System.Text.Json.Serialization;

namespace ImageService.Models
{
    public class User
    {
        public User()
        {
            Images = new HashSet<Image>();
            SavedImages = new List<SavedImage>();
            Likes = new List<Like>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<Image> Images { get; set; }
        [JsonIgnore]
        public virtual ICollection<SavedImage> SavedImages { get; set; }
        [JsonIgnore]
        public virtual ICollection<Like> Likes { get; set; }
    }
}
