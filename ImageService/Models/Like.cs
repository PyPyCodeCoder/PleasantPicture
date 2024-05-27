using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ImageService.Models
{
    public class Like
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Image")]
        public int ImageId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; } = null;
        [JsonIgnore]
        public virtual Image? Image { get; set; } = null;
    }
}
