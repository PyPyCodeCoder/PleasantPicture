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

            // Generate a unique file name
            var fileName = Path.GetRandomFileName();
            var filePath = Path.Combine(_imageDirectory, fileName);

            // Save the file to the file system
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

            // Save the categories
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

            // Ensure the user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Handle file upload
            if (file != null && file.Length > 0)
            {
                // Delete the old file if it exists
                var oldFilePath = Path.Combine(_imageDirectory, image.FilePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Save the new file
                var newFileName = Path.GetRandomFileName();
                var newFilePath = Path.Combine(_imageDirectory, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update the file path in the image entity
                image.FilePath = newFileName;
            }

            // Update other properties
            image.Description = description;
            image.UserId = userId;
            image.User = user;

            // Update categories
            // Remove old categories
            _context.ImageCategories.RemoveRange(image.ImageCategories);

            // Add new categories
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
            
            // Delete the file from the file system
            var filePath = Path.Combine(_imageDirectory, image.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete all related likes
            _context.Likes.RemoveRange(image.Likes);

            // Delete all related saved images
            _context.SavedImages.RemoveRange(image.SavedImages);

            // Delete all related image categories
            _context.ImageCategories.RemoveRange(image.ImageCategories);

            // Delete the image itself
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
