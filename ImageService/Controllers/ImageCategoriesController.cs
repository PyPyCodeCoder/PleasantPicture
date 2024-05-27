using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImageService.Models;

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageCategoriesController : ControllerBase
    {
        private readonly ImageServiceContext _context;

        public ImageCategoriesController(ImageServiceContext context)
        {
            _context = context;
        }

        // GET: api/ImageCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageCategory>>> GetImageCategories()
        {
            return await _context.ImageCategories.ToListAsync();
        }

        // GET: api/ImageCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageCategory>> GetImageCategory(int id)
        {
            var imageCategory = await _context.ImageCategories.FindAsync(id);

            if (imageCategory == null)
            {
                return NotFound();
            }

            return imageCategory;
        }

        // PUT: api/ImageCategories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImageCategory(int id, ImageCategory imageCategory)
        {
            var image = await _context.Images.FindAsync(imageCategory.ImageId);
            var category = await _context.Categories.FindAsync(imageCategory.CategoryId);

            if (image == null || category == null)
            {
                return BadRequest("Invalid ImageId or CategoryId");
            }
            
            if (id != imageCategory.Id)
            {
                return BadRequest();
            }

            _context.Entry(imageCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageCategoryExists(id))
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

        // POST: api/ImageCategories
        [HttpPost]
        public async Task<ActionResult<ImageCategory>> PostImageCategory(ImageCategory imageCategory)
        {
            var image = await _context.Images.FindAsync(imageCategory.ImageId);
            var category = await _context.Categories.FindAsync(imageCategory.CategoryId);

            if (image == null || category == null)
            {
                return BadRequest("Invalid ImageId or CategoryId");
            }

            imageCategory.Image = image;
            imageCategory.Category = category;
            
            _context.ImageCategories.Add(imageCategory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ImageCategoryExists(imageCategory.ImageId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetImageCategory", new { id = imageCategory.ImageId }, imageCategory);
        }

        // DELETE: api/ImageCategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImageCategory(int id)
        {
            var imageCategory = await _context.ImageCategories.FindAsync(id);
            if (imageCategory == null)
            {
                return NotFound();
            }

            _context.ImageCategories.Remove(imageCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageCategoryExists(int id)
        {
            return _context.ImageCategories.Any(e => e.ImageId == id);
        }
    }
}
