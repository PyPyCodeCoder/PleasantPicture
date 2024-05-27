using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ImageService.Models
{
    public class ImageCategory
    {
        public int Id { get; set; }

        [ForeignKey("Image")]
        public int ImageId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Image? Image { get; set; } = null;
        [JsonIgnore]
        public virtual Category? Category { get; set; } = null;
    }
}
