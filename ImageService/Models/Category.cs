using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ImageService.Models
{
    public class Category
    {
        public Category()
        {
            ImageCategories = new List<ImageCategory>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        [Display(Name = "Категорія")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<ImageCategory> ImageCategories { get; set; }
    }
}
