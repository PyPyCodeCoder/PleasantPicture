using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImageService.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ImageServiceContext _context;
        private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

        public ImagesController(ImageServiceContext context)
        {
            _context = context;
            
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            return await _context.Images.Include(i => i.User).ToListAsync();
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            var image = await _context.Images.Include(i => i.User)
                .Include(i => i.Likes)
                .Include(i => i.SavedImages)
                .Include(i => i.ImageCategories)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            return image;
        }
        
        [HttpGet("{id}/data")]
        public async Task<IActionResult> GetImageData(int id)
        {
            var image = await _context.Images.Include(i => i.User)
                .Include(i => i.Likes)
                .Include(i => i.SavedImages)
                .Include(i => i.ImageCategories)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_imageDirectory, image.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Or return a default image placeholder
            }

            var imageData = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(imageData, "image/jpeg");
        }

        // POST: api/Images
        [HttpPost]
        public async Task<ActionResult<Image>> PostImage([FromForm] IFormFile file, [FromForm] string description, [FromForm] int userId, [FromForm] IList<int> categoryIds)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }
            
            if (!IsImage(file))
            {
                return BadRequest("The provided file is not an image");
            }

            // Ensure the user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            var fileName = Path.GetRandomFileName();
            var filePath = Path.Combine(_imageDirectory, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var image = new Image
            {
                Description = description,
                UserId = userId,
                FilePath = fileName,
                User = user
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            
            foreach (var categoryId in categoryIds)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    var imageCategory = new ImageCategory
                    {
                        ImageId = image.Id,
                        CategoryId = category.Id
                    };
                    _context.ImageCategories.Add(imageCategory);
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
        }

        // PUT: api/Images/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImage(int id, [FromForm] IFormFile file, [FromForm] string description, [FromForm] int userId, [FromForm] IList<int> categoryIds)
        {
            if (file != null && file.Length > 0 && !IsImage(file))
            {
                return BadRequest("The provided file is not an image");
            }
            
            var image = await _context.Images
                .Include(i => i.ImageCategories)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null)
            {
                return NotFound();
            }
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            if (file != null && file.Length > 0)
            {
                var oldFilePath = Path.Combine(_imageDirectory, image.FilePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                
                var newFileName = Path.GetRandomFileName();
                var newFilePath = Path.Combine(_imageDirectory, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                image.FilePath = newFileName;
            }
            
            image.Description = description;
            image.UserId = userId;
            image.User = user;
            
            _context.ImageCategories.RemoveRange(image.ImageCategories);
            
            foreach (var categoryId in categoryIds)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    var imageCategory = new ImageCategory
                    {
                        ImageId = image.Id,
                        CategoryId = category.Id
                    };
                    _context.ImageCategories.Add(imageCategory);
                }
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images
                .Include(i => i.Likes)
                .Include(i => i.SavedImages)
                .Include(i => i.ImageCategories)
                .FirstOrDefaultAsync(i => i.Id == id);
        
            if (image == null)
            {
                return NotFound();
            }
            
            var filePath = Path.Combine(_imageDirectory, image.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            
            _context.Likes.RemoveRange(image.Likes);
            
            _context.SavedImages.RemoveRange(image.SavedImages);
            
            _context.ImageCategories.RemoveRange(image.ImageCategories);
            
            _context.Images.Remove(image);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageExists(int id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
        
        private bool IsImage(IFormFile file)
        {
            return file.ContentType.StartsWith("image/");
        }
    }
}
