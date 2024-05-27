using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;
using System.Text.Json.Serialization;

namespace ImageService.Models
{ 
    public class Image
    {
        public Image()
        {
            SavedImages = new List<SavedImage>();
            Likes = new List<Like>();
            ImageCategories = new List<ImageCategory>();
        }

        public int Id { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public string FilePath { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        [JsonIgnore]
        public virtual ICollection<SavedImage> SavedImages { get; set; }
        [JsonIgnore]
        public virtual ICollection<Like> Likes { get; set; }
        [JsonIgnore]
        public virtual ICollection<ImageCategory> ImageCategories { get; set; }
    }
}